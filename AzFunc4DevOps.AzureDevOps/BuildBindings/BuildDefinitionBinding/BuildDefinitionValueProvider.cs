using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    public class BuildDefinitionValueProvider : IValueBinder
    {
        public BuildDefinitionValueProvider(VssConnection connection, BuildDefinitionAttribute attr)
        {
            this._connection = connection;
            this._project = attr.Project;
            this._buildDefinitionId = int.Parse(attr.Id);
        }

        public Type Type => typeof(BuildDefinitionProxy);

        public async Task<object> GetValueAsync()
        {
            var client = await this._connection.GetClientAsync<BuildHttpClient>();

            var definiton = await client.GetDefinitionAsync(this._project, this._buildDefinitionId);

            var proxy = BuildDefinitionProxy.FromDefinition(definiton);

            return proxy;
        }

        public Task SetValueAsync(object value, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public string ToInvokeString()
        {
            return $"{this._project}-{this._buildDefinitionId}";
        }
        
        private readonly VssConnection _connection;
        private readonly string _project;
        private readonly int _buildDefinitionId;
    }
}