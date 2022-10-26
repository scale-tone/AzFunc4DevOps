using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    public class ReleaseEnvironmentStatusCollector : IAsyncCollector<ReleaseEnvironmentStatus>
    {
        public ReleaseEnvironmentStatusCollector(VssConnection connection, ReleaseEnvironmentStatusAttribute attr)
        {
            this._connection = connection;
            this._projectName = attr.ProjectName;
        }

        public async Task AddAsync(ReleaseEnvironmentStatus envStatus, CancellationToken cancellationToken = default)
        {
            if (envStatus == null || envStatus.ReleaseId == 0 || envStatus.ReleaseEnvironmentId == 0)
            {
                return;
            }

            var client = await this._connection.GetClientAsync<ReleaseHttpClient>();

            await client.UpdateReleaseEnvironmentAsync(envStatus, this._projectName, envStatus.ReleaseId, envStatus.ReleaseEnvironmentId);
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        private readonly VssConnection _connection;
        private readonly string _projectName;
    }
}