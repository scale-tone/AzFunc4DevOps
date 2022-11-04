using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    public class ReleaseCreatedWatcherEntity : IGenericWatcherEntity
    {
        #region Entity State

        public List<int> CurrentReleaseIds { get; set; }

        #endregion

        public ReleaseCreatedWatcherEntity(VssConnection connection, TriggerExecutorRegistry executorRegistry)
        {
            this._connection = connection;
            this._executorRegistry = executorRegistry;
        }

        public async Task Watch(DateTimeOffset whenToStop)
        {
            var attribute = (ReleaseCreatedTriggerAttribute)this._executorRegistry.TryGetTriggerAttributeForEntity(Entity.Current.EntityId);
            if (attribute == null)
            {
                return;
            }

            int? definitionId = null;
            if (!string.IsNullOrWhiteSpace(attribute.ReleaseDefinitionId))
            {
                definitionId = int.Parse(attribute.ReleaseDefinitionId);
            }

            var releaseClient = await this._connection.GetClientAsync<ReleaseHttpClient>();

            while (true)
            {
                var releaseIds = (await releaseClient.GetReleasesAsync(
                        project: attribute.Project,
                        definitionId: definitionId,
                        createdBy: attribute.CreatedBy,
                        path: attribute.ReleaseFolderPath,
                        expand: ReleaseExpands.None,
                        // By far just taking the first batch of latest releases. TODO: check what's the actual batch size and whether it is big enough.
                        queryOrder: ReleaseQueryOrder.Descending
                    ))
                    .Select(r => r.Id)
                    .ToList();

                if (this.CurrentReleaseIds == null)
                {
                    // At first run just saving the state and quitting
                    this.CurrentReleaseIds = releaseIds;
                    return;
                }

                var currentReleasesMap = this.CurrentReleaseIds.ToDictionary(id => id);
                var addedReleaseIds = releaseIds.Where(id => !currentReleasesMap.ContainsKey(id)).ToList();

                if (addedReleaseIds.Any())
                {
                    // Triggering functions
                    var invokeTasks = addedReleaseIds.Select(id => this.InvokeFunction(releaseClient, attribute.Project, id)).ToList();
                    await Task.WhenAll(invokeTasks);

                    // Persisting new list of ids in state
                    this.CurrentReleaseIds = releaseIds;

                    // Intentionally quitting, so that our state is persisted immediately.
                    return;
                }

                if (DateTimeOffset.UtcNow > whenToStop) 
                {
                    // Quitting, if it's time to stop
                    return;
                }

                // Delay until next attempt
                await Global.PollingDelay();
            }
        }

        public void Delete()
        {
            Entity.Current.DeleteState();
        }

        private readonly VssConnection _connection;
        private readonly TriggerExecutorRegistry _executorRegistry;

        private async Task InvokeFunction(ReleaseHttpClient client, string project, int releaseId)
        {
            var release = await client.GetReleaseAsync(project, releaseId, expand: SingleReleaseExpands.Tasks);

            var executor = this._executorRegistry.GetExecutorForEntity(Entity.Current.EntityId);

            var data = new TriggeredFunctionData()
            {
                TriggerValue = ReleaseProxy.FromRelease(release)
            };

            var result = await executor.TryExecuteAsync(data, CancellationToken.None);

            if (!result.Succeeded && result.Exception != null)
            {
                throw result.Exception;
            }
        }


        // Required boilerplate
        [FunctionName(Global.FunctionPrefix + nameof(ReleaseCreatedWatcherEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx) => ctx.DispatchAsync<ReleaseCreatedWatcherEntity>();
    }
}