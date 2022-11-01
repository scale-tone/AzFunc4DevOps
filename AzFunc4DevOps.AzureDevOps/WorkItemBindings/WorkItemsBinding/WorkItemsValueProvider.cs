using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    public class WorkItemsValueProvider : IValueBinder
    {
        public WorkItemsValueProvider(VssConnection connection, WorkItemsAttribute attr)
        {
            this._connection = connection;
            this._attribute = attr;
        }

        public Type Type => typeof(IEnumerable<WorkItemProxy>);

        public async Task<object> GetValueAsync()
        {
            var client = await this._connection.GetClientAsync<WorkItemTrackingHttpClient>();

            var query = new Wiql
            {
                Query = $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = '{this._attribute.Project}'" +
                    (string.IsNullOrEmpty(this._attribute.WiqlQueryWhereClause) ? "" : " AND " + this._attribute.WiqlQueryWhereClause)
            };

            var idsResult = await client.QueryByWiqlAsync(query);

            return this.IterateThroughResults(client, idsResult.WorkItems.Select(w => w.Id));
        }

        public Task SetValueAsync(object value, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public string ToInvokeString()
        {
            return $"{this._attribute.Project}-{this._attribute.WiqlQueryWhereClause}";
        }
        
        private readonly VssConnection _connection;
        private readonly WorkItemsAttribute _attribute;

        private IEnumerable<WorkItemProxy> IterateThroughResults(WorkItemTrackingHttpClient client, IEnumerable<int> ids)
        {
            //  200 is max batch size for GetWorkItemsBatchAsync()
            foreach (var idsBatch in ids.ToBatches(200))
            {
                // Intentionally not using async/await here, because we need to return IEnumerable (which might not always be fully iterated)
                var itemsBatch = client.GetWorkItemsBatchAsync(new WorkItemBatchGetRequest { Ids = idsBatch }).Result;

                foreach(var item in itemsBatch)
                {
                    yield return WorkItemProxy.FromWorkItem(item);
                }
            }
        }
    }
}