using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <inheritdoc />
    public class ProjectValueProvider : IValueBinder
    {
        /// <inheritdoc />
        public ProjectValueProvider(VssConnectionFactory connFactory, ProjectAttribute attr)
        {
            this._connection = connFactory.GetVssConnection(attr);
            this._project = attr.Project;
        }

        /// <inheritdoc />
        public Type Type => typeof(ProjectProxy);

        /// <inheritdoc />
        public async Task<object> GetValueAsync()
        {
            var projectClient = await this._connection.GetClientAsync<ProjectHttpClient>();

            var project = await projectClient.GetProject(this._project);

            var proxy = ProjectProxy.FromTeamProject(project);

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
            return $"{this._project}";
        }
        
        private readonly VssConnection _connection;
        private readonly string _project;
    }
}