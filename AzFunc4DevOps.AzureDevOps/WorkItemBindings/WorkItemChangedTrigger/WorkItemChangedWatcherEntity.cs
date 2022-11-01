using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    public class WorkItemChangedWatcherEntity : IGenericWatcherEntity
    {
        #region Entity State

        public Dictionary<int, int> CurrentWorkItemVersions { get; set; }

        #endregion

        public WorkItemChangedWatcherEntity(ILogger log, VssConnection connection, TriggerExecutorRegistry executorRegistry)
        {
            this._connection = connection;
            this._executorRegistry = executorRegistry;
            this._log = log;
        }

        public void Delete()
        {
            Entity.Current.DeleteState();
        }

        public async Task Watch(DateTimeOffset whenToStop)
        {
            var attribute = (WorkItemChangedTriggerAttribute)this._executorRegistry.TryGetTriggerAttributeForEntity(Entity.Current.EntityId);
            if (attribute == null)
            {
                return;
            }

            var workItemClient = await this._connection.GetClientAsync<WorkItemTrackingHttpClient>();

            // Storing here the workItems which function invocation failed for. So that they are only retried during next polling session.
            var failedWorkItemIds = new HashSet<int>();
            while (true)
            {
                string whereClause = $"[System.TeamProject] = '{attribute.ProjectName}'" +
                    (string.IsNullOrEmpty(attribute.WiqlQueryWhereClause) ? "" : " AND " + attribute.WiqlQueryWhereClause);

                var newVersions = await this.GetWorkItemIdsAndRevs(workItemClient, whereClause);

                if (this.CurrentWorkItemVersions == null)
                {
                    // At first run just saving the current snapshot and quitting
                    this.CurrentWorkItemVersions = newVersions;
                    return;
                }

                var itemVersions = new Dictionary<int, int>();

                foreach (var item in newVersions)
                {
                    this.CurrentWorkItemVersions.TryGetValue(item.Key, out var curVersion);
                    var newVersion = item.Value;

                    if (newVersion != curVersion && !failedWorkItemIds.Contains(item.Key))
                    {
                        try
                        {
                            // Intentionally using await, to distribute the load against Azure DevOps
                            await this.InvokeFunctionForWorkItemVersions(workItemClient, attribute, item.Key, curVersion, newVersion);

                            // Bumping up the known version
                            curVersion = newVersion;
                        }
                        catch (Exception ex)
                        {                            
                            this._log.LogError(ex, $"WorkItemChangedTrigger failed for WorkItem #{item.Key}");

                            // Memorizing this workItem as failed, so that it is only retried next time.
                            failedWorkItemIds.Add(item.Key);
                        }
                    }

                    itemVersions[item.Key] = curVersion;
                }

                // Setting new state
                this.CurrentWorkItemVersions = itemVersions;

                // Explicitly persisting current state
                Entity.Current.SetState(this);

                if (DateTimeOffset.UtcNow > whenToStop) 
                {
                    // Quitting, if it's time to stop
                    return;
                }

                // Delay until next attempt
                await Global.DelayForAboutASecond();
            }
        }

        private async Task<Dictionary<int, int>> GetWorkItemIdsAndRevs(WorkItemTrackingHttpClient client, string whereClause)
        {
            //TODO: investigate whether it's possible to get versions with a single call
            var query = new Wiql
            {
                Query = $"SELECT [System.Id] FROM workitems WHERE {whereClause}"
            };

            var idsResult = await client.QueryByWiqlAsync(query);

            // Fetching workItem revs in batches (200 is the maximum for GetWorkItemsBatchAsync())
            var workItemIdBatches = idsResult.WorkItems.Select(w => w.Id).ToList().ToBatches(200);

            var tasks = workItemIdBatches.Select(batch => client.GetWorkItemsBatchAsync(new WorkItemBatchGetRequest { Ids = batch }));
            var workItems = (await Task.WhenAll(tasks)).SelectMany(b => b);

            var result = workItems.ToDictionary(w => w.Id.Value, w => w.Rev.Value);
            return result;
        }

        private async Task InvokeFunctionForWorkItemVersions(WorkItemTrackingHttpClient client, WorkItemChangedTriggerAttribute attr, int workItemId, int fromVersion, int tillVersion)
        {
            if (fromVersion <= 0 && tillVersion == 1)
            {
                // The workItem has just been added, so doing nothing
                return;
            }

            if (fromVersion <= 0)
            {
                fromVersion = 1;
            }

            if (string.IsNullOrWhiteSpace(attr.FromValue) && string.IsNullOrWhiteSpace(attr.ToValue))
            {
                await this.InvokeFunctionForWorkItemsIfChanged(client, attr, workItemId, fromVersion, tillVersion);
            }
            else
            {
                // Need to go through all the versions
                for (int i = fromVersion; i < tillVersion; i++)
                {
                    await this.InvokeFunctionForWorkItemsIfChanged(client, attr, workItemId, i, i + 1);
                }
            }
        }

        private async Task InvokeFunctionForWorkItemsIfChanged(WorkItemTrackingHttpClient client, WorkItemChangedTriggerAttribute attr, int workItemId, int oldVersion, int newVersion)
        {
            var oldItem = await client.GetRevisionAsync(attr.ProjectName, workItemId, oldVersion, WorkItemExpand.All);
            var newItem = await client.GetRevisionAsync(attr.ProjectName, workItemId, newVersion, WorkItemExpand.All);

            bool hasChanged = false;

            if (string.IsNullOrWhiteSpace(attr.FieldName))
            {
                // At this point we can be sure that something has indeed changed in the workItem
                hasChanged = true;
            }
            else
            {
                // Watching only this particular field

                oldItem.Fields.TryGetValue(attr.FieldName, out var oldValue);
                newItem.Fields.TryGetValue(attr.FieldName, out var newValue);

                hasChanged = oldValue?.ToString() != newValue?.ToString();

                if (!string.IsNullOrWhiteSpace(attr.FromValue))
                {
                    hasChanged = hasChanged && 
                        oldValue?.ToString() == attr.FromValue.ToString();
                }

                if (!string.IsNullOrWhiteSpace(attr.ToValue))
                {
                    hasChanged = hasChanged && 
                        newValue?.ToString() == attr.ToValue.ToString();
                }
            }

            if (hasChanged) 
            {
                // Passing the most recent version intentionally, so that it can then be modified and saved by the client code.
                var latestItem = await client.GetWorkItemAsync(attr.ProjectName, newItem.Id.Value, null, null, WorkItemExpand.All);

                await this.InvokeFunction(oldItem, latestItem);
            }
        }

        private async Task InvokeFunction(WorkItem oldItem, WorkItem newItem)
        {
            var executor = this._executorRegistry.GetExecutorForEntity(Entity.Current.EntityId);

            var data = new TriggeredFunctionData()
            {
                TriggerValue = new WorkItemChange(oldItem, newItem)
            };

            var result = await executor.TryExecuteAsync(data, CancellationToken.None);

            if (!result.Succeeded && result.Exception != null)
            {
                throw result.Exception;
            }
        }

        private readonly VssConnection _connection;
        private readonly TriggerExecutorRegistry _executorRegistry;
        private readonly ILogger _log;
        

        // Required boilerplate
        [FunctionName(Global.FunctionPrefix + nameof(WorkItemChangedWatcherEntity))]
        public static Task Run(
            [EntityTrigger] IDurableEntityContext ctx,
            ILogger log
        ) => ctx.DispatchAsync<WorkItemChangedWatcherEntity>(log);
    }
}