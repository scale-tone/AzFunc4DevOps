using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    public class WorkItemCreatedWatcherEntity : IGenericWatcherEntity
    {
        #region Entity State

        public List<int> CurrentWorkItemIds { get; set; }

        #endregion

        public WorkItemCreatedWatcherEntity(VssConnection connection, TriggerExecutorRegistry executorRegistry)
        {
            this._connection = connection;
            this._executorRegistry = executorRegistry;
        }

        public async Task Watch(DateTimeOffset whenToStop)
        {
            var attribute = (WorkItemCreatedTriggerAttribute)this._executorRegistry.TryGetTriggerAttributeForEntity(Entity.Current.EntityId);
            if (attribute == null)
            {
                return;
            }

            var workItemClient = await this._connection.GetClientAsync<WorkItemTrackingHttpClient>();

            while (true)
            {
                // Querying for Ids
                var query = new Wiql
                {
                    Query = $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = '{attribute.Project}'" +
                        (string.IsNullOrEmpty(attribute.WiqlQueryWhereClause) ? "" : " AND " + attribute.WiqlQueryWhereClause)
                };

                var result = await workItemClient.QueryByWiqlAsync(query);
                var workItemIds = result.WorkItems.Select(w => w.Id).ToList();

                if (this.CurrentWorkItemIds == null)
                {
                    // At first run just saving the state and quitting
                    this.CurrentWorkItemIds = workItemIds;
                    return;
                }

                var currentWorkItemsMap = this.CurrentWorkItemIds.ToDictionary(id => id);
                var addedItems = workItemIds.Where(id => !currentWorkItemsMap.ContainsKey(id)).ToList();

                if (addedItems.Any())
                {
                    // Triggering functions
                    var invokeTasks = addedItems.Select(id => this.InvokeFunction(workItemClient, attribute.Project, id)).ToList();
                    await Task.WhenAll(invokeTasks);

                    // Persisting new list of ids in state
                    this.CurrentWorkItemIds = workItemIds;

                    // Intentionally quitting, so that our state is persisted immediately.
                    return;
                }

                if (DateTimeOffset.UtcNow > whenToStop) 
                {
                    // Quitting, if it's time to stop
                    return;
                }

                // Delay until next attempt
                await Global.DelayForAboutASecond();
            }
        }

        public void Delete()
        {
            Entity.Current.DeleteState();
        }

        private readonly VssConnection _connection;
        private readonly TriggerExecutorRegistry _executorRegistry;

        private async Task InvokeFunction(WorkItemTrackingHttpClient workItemClient, string projectName, int workItemId)
        {
            var workItem = await workItemClient.GetWorkItemAsync(projectName, workItemId, null, null, WorkItemExpand.All);

            var executor = this._executorRegistry.GetExecutorForEntity(Entity.Current.EntityId);

            var data = new TriggeredFunctionData()
            {
                TriggerValue = WorkItemProxy.FromWorkItem(workItem)
            };

            //TODO: handle exceptions
            await executor.TryExecuteAsync(data, CancellationToken.None);
        }

        // Required boilerplate
        [FunctionName(Global.FunctionPrefix + nameof(WorkItemCreatedWatcherEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx) => ctx.DispatchAsync<WorkItemCreatedWatcherEntity>();
    }
}