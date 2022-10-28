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

            var release = await client.GetReleaseAsync(this._projectName, envStatus.ReleaseId);

            var env = release.Environments.SingleOrDefault(e => e.Id == envStatus.ReleaseEnvironmentId);

            if (env == null)
            {
                throw new ArgumentException($"Environment #{envStatus.ReleaseEnvironmentId} not found in release #{envStatus.ReleaseId}");
            }

            if (envStatus.Variables.Count > 0)
            {
                // Those variables that belong to release level - updating them at release level
                foreach(var kv in release.Variables)
                {
                    if (envStatus.Variables.TryGetValue(kv.Key, out var newValue))
                    {
                        // Only updating the value, just in case
                        kv.Value.Value = newValue.Value;

                        envStatus.Variables.Remove(kv.Key);
                    }
                }

                // Placing the rest onto environment level
                foreach(var kv in envStatus.Variables)
                {
                    if (env.Variables.TryGetValue(kv.Key, out var oldValue))
                    {
                        // Only updating the value, just in case
                        oldValue.Value = kv.Value.Value;
                    }
                    else
                    {
                        env.Variables[kv.Key] = kv.Value;
                    }
                }

                await client.UpdateReleaseAsync(release, this._projectName, envStatus.ReleaseId);

                envStatus.Variables.Clear();
            }

            if (envStatus.Status == EnvironmentStatus.Undefined || envStatus.Status == env.Status)
            {
                return;
            }

            // This method effectively only able to change the status. And only if the status is really being changed 
            // (otherwise it says it can't change the status that is not changing).
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