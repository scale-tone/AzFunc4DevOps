using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;

namespace AzFunc4DevOps.AzureDevOps
{
    internal class WorkItemWatcherEntity : IWorkItemWatcherEntity
    {
        #region Entity State

        public WorkItem CurrentWorkItem { get; set; }

        #endregion

        public WorkItemWatcherEntity(VssConnection connection, TriggerExecutorRegistry executorRegistry)
        {
            this._connection = connection;
            this._executorRegistry = executorRegistry;
        }

        public async Task Watch(WorkItemWatcherParams watcherParams)
        {
            // Did some research - and this seems to be the correct pattern.
            // Client instances are cached inside VssConnection, it then returns same singleton instance.
            // Therefore client instances should not be disposed.
            var workItemClient = await this._connection.GetClientAsync<WorkItemTrackingHttpClient>();

            do
            {
                WorkItem workItem;
                try
                {
                    workItem = await workItemClient.GetWorkItemAsync(watcherParams.ProjectName, watcherParams.WorkItemId, null, null, WorkItemExpand.All);
                }
                catch (VssServiceException ex)
                {
                    if (ex.Message.Contains("TF401232")) 
                    {
                        // The item was deleted. Removing our state and quitting

                        Entity.Current.DeleteState();
                        return;
                    }

                    throw;
                }

                if (this.CurrentWorkItem == null)
                {
                    // At first run just storing the current state and doing nothing
                    this.CurrentWorkItem = workItem;
                    return;
                }

                if (await this.CheckIfChangedAndTriggerFunctionIfNeeded(this.CurrentWorkItem, workItem, watcherParams))
                {
                    // Only modifying our state after successfully triggering the function
                    this.CurrentWorkItem = workItem;

                    // Intentionally quitting, so that our state is persisted immediately.
                    return;
                }

                if (DateTimeOffset.UtcNow > watcherParams.WhenToStop) 
                {
                    // Quitting, if it's time to stop
                    return;
                }

                // Delay until next attempt
                await Global.DelayForAboutASecond();
            }
            while (true);
        }

        public void Delete()
        {
            Entity.Current.DeleteState();
        }

        private readonly VssConnection _connection;
        private readonly TriggerExecutorRegistry _executorRegistry;

        private async Task<bool> CheckIfChangedAndTriggerFunctionIfNeeded(WorkItem oldItem, WorkItem newItem, WorkItemWatcherParams watcherParams)
        {
            if (string.IsNullOrEmpty(watcherParams.FieldName)) 
            {
                // Matching entire JSONs
                var oldItemJson = JsonConvert.SerializeObject(oldItem);
                var newItemJson = JsonConvert.SerializeObject(newItem);

                // If didn't change, quitting
                if (oldItemJson == newItemJson) 
                {
                    return false;
                }

                await this.InvokeFunction(watcherParams.ParentEntityId, oldItem, newItem);
            }
            else
            {
                // Watching only this particular field

                oldItem.Fields.TryGetValue(watcherParams.FieldName, out var oldValue);
                newItem.Fields.TryGetValue(watcherParams.FieldName, out var newValue);

                bool valueHasChanged = oldValue?.ToString() != newValue?.ToString();

                if (!valueHasChanged)
                {
                    return false;
                }

                Func<bool> oldValuePredicate;
                if (watcherParams.FromValue == null)
                {
                    oldValuePredicate = () => { return valueHasChanged; };
                }
                else
                {
                    oldValuePredicate = () => 
                    {
                        return oldValue?.ToString() == watcherParams.FromValue?.ToString();
                    };                 
                }

                Func<bool> newValuePredicate;
                if (watcherParams.ToValue == null)
                {
                    newValuePredicate = () => { return valueHasChanged; };
                }
                else
                {
                    newValuePredicate = () => 
                    {
                        return newValue?.ToString() == watcherParams.ToValue?.ToString();
                    };                    
                }

                // Only triggering the function if field conditions are both satisfied
                if (oldValuePredicate() && newValuePredicate())
                {
                    await this.InvokeFunction(watcherParams.ParentEntityId, oldItem, newItem);
                }
            }

            return true;
        }

        private async Task InvokeFunction(EntityId parentEntityId, WorkItem oldItem, WorkItem newItem)
        {
            var executor = this._executorRegistry.GetExecutorForEntity(parentEntityId);

            var data = new TriggeredFunctionData()
            {
                TriggerValue = new WorkItemChange(oldItem, newItem)
            };

            await executor.TryExecuteAsync(data, CancellationToken.None);
        }

        // Required boilerplate
        [FunctionName(Global.FunctionPrefix + nameof(WorkItemWatcherEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx) => ctx.DispatchAsync<WorkItemWatcherEntity>();
    }
}