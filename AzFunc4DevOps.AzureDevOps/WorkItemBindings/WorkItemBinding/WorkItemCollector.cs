using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    public class WorkItemCollector : IAsyncCollector<WorkItemProxy>
    {
        public WorkItemCollector(VssConnection connection, WorkItemAttribute attr)
        {
            this._connection = connection;
            this._projectName = attr.ProjectName;
        }

        public async Task AddAsync(WorkItemProxy workItem, CancellationToken cancellationToken = default)
        {
            if (workItem == null)
            {
                return;
            }

            var patchDoc = workItem.GetJsonPatchDocument();

            if (patchDoc.Count <= 0)
            {
                return;
            }

            var workItemClient = await this._connection.GetClientAsync<WorkItemTrackingHttpClient>();

            if (workItem.Id.HasValue)
            {
                // Updating existing workItem, but only if it doesn't come from input binding (to prevent double updates)
                if (!workItem.CreatedByValueProvider)
                {
                    await workItemClient.UpdateWorkItemAsync(patchDoc, workItem.Id.Value);
                }
            }
            else
            {
                // Creating a new workItem
                await workItemClient.CreateWorkItemAsync(patchDoc, this._projectName, workItem.WorkItemType);
            }
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        private readonly VssConnection _connection;
        private readonly string _projectName;
    }
}