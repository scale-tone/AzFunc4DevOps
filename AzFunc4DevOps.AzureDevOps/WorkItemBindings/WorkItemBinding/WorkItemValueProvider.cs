using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
        /// <inheritdoc />
    public class WorkItemValueProvider : IValueBinder
    {
        /// <inheritdoc />
        public WorkItemValueProvider(VssConnectionFactory connFactory, WorkItemAttribute attr)
        {
            this._connection = connFactory.GetVssConnection(attr);
            this._project = attr.Project;
            this._id = int.Parse(attr.Id);
            this.Type = attr.GetSpecificWorkItemType();
        }

        /// <inheritdoc />
        public Type Type { get; private set; }

        /// <inheritdoc />
        public async Task<object> GetValueAsync()
        {
            var workItemClient = await this._connection.GetClientAsync<WorkItemTrackingHttpClient>();

            var workItem = await workItemClient.GetWorkItemAsync(this._project, this._id, expand: WorkItemExpand.All);

            var proxy = WorkItemProxy.FromWorkItem(workItem, this.Type);

            return proxy;
        }

        /// <inheritdoc />
        public Task SetValueAsync(object value, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public string ToInvokeString()
        {
            return $"{this._project}-{this._id}";
        }
        
        private readonly VssConnection _connection;
        private readonly string _project;
        private readonly int _id;
    }
}