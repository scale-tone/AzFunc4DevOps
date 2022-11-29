using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <inheritdoc />
    public class BuildDefinitionValueProvider : IValueBinder
    {
        /// <inheritdoc />
        public BuildDefinitionValueProvider(VssConnectionFactory connFactory, BuildDefinitionAttribute attr)
        {
            this._connection = connFactory.GetVssConnection(attr);
            this._project = attr.Project;
            this._buildDefinitionId = int.Parse(attr.Id);
        }

        /// <inheritdoc />
        public Type Type => typeof(BuildDefinitionProxy);

        /// <inheritdoc />
        public async Task<object> GetValueAsync()
        {
            var client = await this._connection.GetClientAsync<BuildHttpClient>();

            var definiton = await client.GetDefinitionAsync(this._project, this._buildDefinitionId);

            var proxy = BuildDefinitionProxy.FromDefinition(definiton);

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
            return $"{this._project}-{this._buildDefinitionId}";
        }
        
        private readonly VssConnection _connection;
        private readonly string _project;
        private readonly int _buildDefinitionId;
    }
}