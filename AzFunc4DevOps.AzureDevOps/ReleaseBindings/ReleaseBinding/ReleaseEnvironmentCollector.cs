using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    public class ReleaseEnvironmentCollector : IAsyncCollector<ReleaseEnvironment>
    {
        public ReleaseEnvironmentCollector(VssConnection connection, ReleaseEnvironmentAttribute attr)
        {
            this._connection = connection;
            this._projectName = attr.ProjectName;
        }

        public async Task AddAsync(ReleaseEnvironment releaseStage, CancellationToken cancellationToken = default)
        {
            if (releaseStage == null || releaseStage.Id == 0 || releaseStage.ReleaseId == 0)
            {
                return;
            }

            var client = await this._connection.GetClientAsync<ReleaseHttpClient>();

            var data = new ReleaseEnvironmentUpdateMetadata()
            {
                Status = releaseStage.Status,
                ScheduledDeploymentTime = releaseStage.ScheduledDeploymentTime
            };

            await client.UpdateReleaseEnvironmentAsync(data, this._projectName, releaseStage.ReleaseId, releaseStage.Id);
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        private readonly VssConnection _connection;
        private readonly string _projectName;
    }
}