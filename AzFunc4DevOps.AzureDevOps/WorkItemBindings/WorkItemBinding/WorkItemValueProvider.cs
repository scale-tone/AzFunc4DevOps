using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    public class WorkItemValueProvider : IValueBinder
    {
        public WorkItemValueProvider(VssConnection connection, WorkItemAttribute attr)
        {
            this._connection = connection;
            this._projectName = attr.ProjectName;
            this._id = int.Parse(attr.Id);
            this.Type = attr.GetSpecificWorkItemType();
        }

        public Type Type { get; private set; }

        public async Task<object> GetValueAsync()
        {
            var workItemClient = await this._connection.GetClientAsync<WorkItemTrackingHttpClient>();

            var workItem = await workItemClient.GetWorkItemAsync(this._projectName, this._id, expand: WorkItemExpand.All);

            var proxy = WorkItemProxy.FromWorkItem(workItem, this.Type);

            return proxy;
        }

        public Task SetValueAsync(object value, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public string ToInvokeString()
        {
            return $"{this._projectName}-{this._id}";
        }
        
        private readonly VssConnection _connection;
        private readonly string _projectName;
        private readonly int _id;
    }
}