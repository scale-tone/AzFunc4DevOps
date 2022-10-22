using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    public class ProjectValueProvider : IValueBinder
    {
        public ProjectValueProvider(VssConnection connection, ProjectAttribute attr)
        {
            this._connection = connection;
            this._projectName = attr.ProjectName;
        }

        public Type Type => typeof(ProjectProxy);

        public async Task<object> GetValueAsync()
        {
            var projectClient = await this._connection.GetClientAsync<ProjectHttpClient>();

            var project = await projectClient.GetProject(this._projectName);

            var proxy = ProjectProxy.FromTeamProject(project);

            return proxy;
        }

        public Task SetValueAsync(object value, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public string ToInvokeString()
        {
            return $"{this._projectName}";
        }
        
        private readonly VssConnection _connection;
        private readonly string _projectName;
    }
}