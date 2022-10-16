using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    public class WorkItemValueProvider : IValueBinder
    {
        public WorkItemValueProvider(VssConnection connection, WorkItemAttribute attr)
        {
            this._connection = connection;
            this._projectName = attr.ProjectName;
            this._workItemId = int.Parse(attr.WorkItemId);
        }

        public Type Type => typeof(WorkItemProxy);

        public async Task<object> GetValueAsync()
        {
            var workItemClient = await this._connection.GetClientAsync<WorkItemTrackingHttpClient>();

            var workItem = await workItemClient.GetWorkItemAsync(this._projectName, this._workItemId);

            var proxy = WorkItemProxy.FromWorkItem(workItem);

            proxy.CreatedByValueProvider = true;

            return proxy;
        }

        public async Task SetValueAsync(object value, CancellationToken cancellationToken)
        {
            var workItem = (WorkItemProxy)value;

            var patchDoc = workItem.GetJsonPatchDocument();

            if (patchDoc.Count <= 0)
            {
                // If wasn't changed, doing nothing
                return;
            }

            var workItemClient = await this._connection.GetClientAsync<WorkItemTrackingHttpClient>();

            await workItemClient.UpdateWorkItemAsync(patchDoc, workItem.Id.Value);
        }

        public string ToInvokeString()
        {
            return $"{this._projectName}-{this._workItemId}";
        }
        
        private readonly VssConnection _connection;
        private readonly string _projectName;
        private readonly int _workItemId;
    }
}