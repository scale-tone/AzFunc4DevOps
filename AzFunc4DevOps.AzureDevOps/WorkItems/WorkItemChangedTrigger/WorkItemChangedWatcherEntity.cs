using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    public class WorkItemChangedWatcherEntity : IGenericWatcherEntity
    {
        #region Entity State

        public List<int> CurrentWorkItemIds { get; set; }

        #endregion

        public WorkItemChangedWatcherEntity(VssConnection connection, TriggerExecutorRegistry executorRegistry)
        {
            this._connection = connection;
            this._executorRegistry = executorRegistry;
        }

        public async Task Watch(DateTimeOffset whenToStop)
        {
            var attribute = (WorkItemChangedTriggerAttribute)this._executorRegistry.TryGetTriggerAttributeForEntity(Entity.Current.EntityId);
            if (attribute == null)
            {
                return;
            }

            var workItemClient = await this._connection.GetClientAsync<WorkItemTrackingHttpClient>();

            // Querying for Ids
            var query = new Wiql
            {
                Query = $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = '{attribute.ProjectName}'" +
                    (string.IsNullOrEmpty(attribute.WiqlQueryWhereClause) ? "" : " AND " + attribute.WiqlQueryWhereClause)
            };

            var result = await workItemClient.QueryByWiqlAsync(query);
            var workItemIds = result.WorkItems.Select(w => w.Id).ToList();

            // Pinging all child entities
            foreach (var id in workItemIds)
            {
                Entity.Current.SignalEntity<IWorkItemWatcherEntity>(
                    this.GetChildEntityId(attribute.ProjectName, id),
                    e => e.Watch(new WorkItemWatcherParams
                    { 
                        ProjectName = attribute.ProjectName,
                        FieldName = attribute.FieldName,
                        FromValue = attribute.FromValue,
                        ToValue = attribute.ToValue,
                        WorkItemId = id, 
                        ParentEntityId = Entity.Current.EntityId,
                        WhenToStop = whenToStop
                    })
                );
            }

            if (this.CurrentWorkItemIds != null)
            {
                // Checking if any of workItems were removed
                var workItemsMap = workItemIds.ToDictionary(id => id);
                foreach (var id in this.CurrentWorkItemIds)
                {
                    if (!workItemsMap.ContainsKey(id))
                    {
                        // Dropping child entity for this removed workItem
                        Entity.Current.SignalEntity<IWorkItemWatcherEntity>(
                            this.GetChildEntityId(attribute.ProjectName, id),
                            e => e.Delete()
                        );
                    }
                }
            }

            this.CurrentWorkItemIds = workItemIds;
        }

        private readonly VssConnection _connection;
        private readonly TriggerExecutorRegistry _executorRegistry;

        private EntityId GetChildEntityId(string projectName, int workItemId)
        {
            return new EntityId(Global.FunctionPrefix + nameof(WorkItemWatcherEntity), $"{projectName}-{workItemId}".GetMd5Hash());
        }

        // Required boilerplate
        [FunctionName(Global.FunctionPrefix + nameof(WorkItemChangedWatcherEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx) => ctx.DispatchAsync<WorkItemChangedWatcherEntity>();
    }
}