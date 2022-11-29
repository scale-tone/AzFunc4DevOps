using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <inheritdoc />
    public class ReleaseEnvironmentCollector : IAsyncCollector<ReleaseEnvironmentProxy>
    {
        /// <inheritdoc />
        public ReleaseEnvironmentCollector(VssConnectionFactory connFactory, ReleaseEnvironmentAttribute attr)
        {
            this._connection = connFactory.GetVssConnection(attr);
            this._project = attr.Project;
        }

        /// <inheritdoc />
        public async Task AddAsync(ReleaseEnvironmentProxy releaseStage, CancellationToken cancellationToken = default)
        {
            if (releaseStage == null || releaseStage.Id == 0 || releaseStage.ReleaseId == 0)
            {
                return;
            }

            var client = await this._connection.GetClientAsync<ReleaseHttpClient>();

            // First need to update the whole release object, because it's the only way to modify e.g. variable values
            var release = await client.GetReleaseAsync(this._project, releaseStage.ReleaseId);

            var oldReleaseStage = release.Environments.SingleOrDefault(e => e.Id == releaseStage.Id);
            if (oldReleaseStage == null)
            {
                throw new ArgumentException($"Environment #${releaseStage.Id} not found in release #{release.Id}");
            }

            release.Environments[release.Environments.IndexOf(oldReleaseStage)] = releaseStage;

            await client.UpdateReleaseAsync(release, this._project, release.Id);

            // Now applying the stage status, if it was changed by the client code
            if (oldReleaseStage.Status != releaseStage.Status)
            {
                var data = new ReleaseEnvironmentUpdateMetadata()
                {
                    Status = releaseStage.Status
                };

                await client.UpdateReleaseEnvironmentAsync(data, this._project, releaseStage.ReleaseId, releaseStage.Id);
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