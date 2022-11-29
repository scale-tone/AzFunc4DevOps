using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <inheritdoc />
    public class WorkItemCollector : IAsyncCollector<WorkItemProxy>
    {
        /// <inheritdoc />
        public WorkItemCollector(VssConnectionFactory connFactory, WorkItemAttribute attr)
        {
            this._connection = connFactory.GetVssConnection(attr);
            this._project = attr.Project;
        }

        /// <inheritdoc />
        public async Task AddAsync(WorkItemProxy workItem, CancellationToken cancellationToken = default)
        {
            if (workItem == null)
            {
                return;
            }

            var patchDoc = workItem.GetJsonPatchDocument(out var originalRev);

            if (patchDoc.Count <= 0)
            {
                return;
            }

            // Adding optimistic locking
            if (originalRev.HasValue)
            {
                patchDoc.Add(new JsonPatchOperation()
                {
                    Operation = Operation.Test,
                    Path = "/rev",
                    Value = originalRev.Value
                });
            }

            var workItemClient = await this._connection.GetClientAsync<WorkItemTrackingHttpClient>();

            if (workItem.Id.HasValue)
            {
                await workItemClient.UpdateWorkItemAsync(patchDoc, workItem.Id.Value);
            }
            else
            {
                // Creating a new workItem
                var createdItem = await workItemClient.CreateWorkItemAsync(patchDoc, this._project, workItem.WorkItemType);

                workItem.Id = createdItem.Id;
            }
        }

        /// <inheritdoc />
        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        private readonly VssConnection _connection;
        private readonly string _project;
    }
}