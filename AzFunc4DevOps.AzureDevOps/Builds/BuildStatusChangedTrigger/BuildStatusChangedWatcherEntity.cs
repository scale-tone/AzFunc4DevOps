using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    public class BuildStatusChangedWatcherEntity : IGenericWatcherEntity<GenericWatcherEntityParams>
    {
        #region Entity State

        public Dictionary<int, BuildStatus> CurrentStatuses { get; set; }

        #endregion

        public BuildStatusChangedWatcherEntity(ILogger log, VssConnection connection, TriggerExecutorRegistry executorRegistry)
        {
            this._connection = connection;
            this._executorRegistry = executorRegistry;
            this._log = log;
        }

        public void Delete()
        {
            Entity.Current.DeleteState();
        }

        public async Task Watch(GenericWatcherEntityParams watcherParams)
        {
            var attribute = (BuildStatusChangedTriggerAttribute)this._executorRegistry.TryGetTriggerAttributeForEntity(Entity.Current.EntityId);
            if (attribute == null)
            {
                return;
            }

            var buildDefinitionIds = string.IsNullOrWhiteSpace(attribute.BuildDefinitionIds) ? 
                null : 
                attribute.BuildDefinitionIds.Split(',').Select(s => int.Parse(s.Trim())).ToList();

            var agentPoolIds = string.IsNullOrWhiteSpace(attribute.AgentPoolIds) ? 
                null : 
                attribute.AgentPoolIds.Split(',').Select(s => int.Parse(s.Trim())).ToList();

            var buildReasons = string.IsNullOrWhiteSpace(attribute.BuildReasons) ?
                (BuildReason?)null :
                attribute.BuildReasons.Split(',')
                    .Select(s => (BuildReason)Enum.Parse(typeof(BuildReason), s.Trim()))
                    .Aggregate((c, s) => c | s);

            // There's some complicated heuristics below, that detects cases when we 'miss' some particular build state
            // (when the build runs too quickly).
            // This flag is to ensure that this heuristics doesn't cause the trigger to fire twice.
            var shouldBeTriggeredOnlyOnce = (!string.IsNullOrWhiteSpace(attribute.FromValue)) || (!string.IsNullOrWhiteSpace(attribute.ToValue));

            var buildClient = await this._connection.GetClientAsync<BuildHttpClient>();

            // Storing here the items which function invocation failed for. So that they are only retried during next polling session.
            var failedBuildIds = new HashSet<int>();
            while (true)
            {
                var builds = await buildClient.GetBuildsAsync(
                    project: attribute.ProjectName,
                    definitions: buildDefinitionIds,
                    queues: agentPoolIds,
                    buildNumber: attribute.BuildNumber,
                    requestedFor: attribute.RequestedFor,
                    reasonFilter: buildReasons,
                    repositoryId: attribute.RepositoryId
                );

                if (this.CurrentStatuses == null)
                {
                    // At first run just saving the current snapshot and quitting
                    this.CurrentStatuses = builds.ToDictionary(b => b.Id, b => b.Status.Value);;
                    return;
                }

                var newStatuses = new Dictionary<int, BuildStatus>();

                foreach (var build in builds)
                {
                    this.CurrentStatuses.TryGetValue(build.Id, out var curStatus);
                    var newStatus = build.Status ?? BuildStatus.None;

                    if (curStatus != BuildStatus.All && newStatus != curStatus && !failedBuildIds.Contains(build.Id))
                    {
                        try
                        {
                            if (this.CheckIfShouldBeTriggered(attribute, build))
                            {
                                // Intentionally using await, to distribute the load against Azure DevOps
                                await this.InvokeFunction(build);

                                // Marking that function has already been triggered for this build
                                curStatus = shouldBeTriggeredOnlyOnce ? BuildStatus.All : newStatus;
                            }
                            else
                            {
                                // Just bumping up the known version
                                curStatus = newStatus;
                            }
                        }
                        catch (Exception ex)
                        {                            
                            this._log.LogError(ex, $"BuildStatusChangedTrigger failed for build #{build.Id}");

                            // Memorizing this workItem as failed, so that it is only retried next time.
                            failedBuildIds.Add(build.Id);
                        }
                    }

                    newStatuses[build.Id] = curStatus;
                }

                // Setting new state
                this.CurrentStatuses = newStatuses;

                // Explicitly persisting current state
                Entity.Current.SetState(this);

                if (DateTimeOffset.UtcNow > watcherParams.WhenToStop) 
                {
                    // Quitting, if it's time to stop
                    return;
                }

                // Delay until next attempt
                await Global.DelayForAboutASecond();
            }
        }

        private readonly VssConnection _connection;
        private readonly TriggerExecutorRegistry _executorRegistry;
        private readonly ILogger _log;

        private bool CheckIfShouldBeTriggered(BuildStatusChangedTriggerAttribute attr, Build build)
        {
            bool isChanged = true;

            var status = build.Status ?? BuildStatus.None;

            if (!string.IsNullOrWhiteSpace(attr.FromValue))
            {
                var fromStatus = (BuildStatus)Enum.Parse(typeof(BuildStatus), attr.FromValue);

                // Checking that current status is _more_ than fromStatus
                switch (fromStatus)
                {
                    case BuildStatus.NotStarted:
                        isChanged = isChanged && status.In(BuildStatus.Postponed, BuildStatus.InProgress, BuildStatus.Cancelling, BuildStatus.Completed);
                    break;
                    case BuildStatus.Postponed:
                        isChanged = isChanged && status.In(BuildStatus.InProgress, BuildStatus.Cancelling, BuildStatus.Completed);
                    break;
                    case BuildStatus.InProgress:
                        isChanged = isChanged && status.In(BuildStatus.Cancelling, BuildStatus.Completed);
                    break;
                    case BuildStatus.Cancelling:
                        isChanged = isChanged && status.In(BuildStatus.Completed);
                    break;
                    case BuildStatus.Completed:
                        isChanged = false;
                    break;
                }
            }

            if (!string.IsNullOrWhiteSpace(attr.ToValue))
            {
                var toStatus = (BuildStatus)Enum.Parse(typeof(BuildStatus), attr.ToValue);

                // Checking that current status is _more_or_equal_ than toStatus
                switch (toStatus)
                {
                    case BuildStatus.None:
                        isChanged = false;
                    break;
                    case BuildStatus.NotStarted:
                        isChanged = isChanged && status.In(BuildStatus.NotStarted, BuildStatus.Postponed, BuildStatus.InProgress, BuildStatus.Cancelling, BuildStatus.Completed);
                    break;
                    case BuildStatus.Postponed:
                        isChanged = isChanged && status.In(BuildStatus.Postponed, BuildStatus.InProgress, BuildStatus.Cancelling, BuildStatus.Completed);
                    break;
                    case BuildStatus.InProgress:
                        isChanged = isChanged && status.In(BuildStatus.InProgress, BuildStatus.Cancelling, BuildStatus.Completed);
                    break;
                    case BuildStatus.Cancelling:
                        isChanged = isChanged && status.In(BuildStatus.Cancelling, BuildStatus.Completed);
                    break;
                    case BuildStatus.Completed:
                        isChanged = isChanged && status.In(BuildStatus.Completed);
                    break;
                }
            }

            return isChanged;
        }

        private async Task InvokeFunction(Build build)
        {
            var executor = this._executorRegistry.GetExecutorForEntity(Entity.Current.EntityId);

            var data = new TriggeredFunctionData()
            {
                TriggerValue = BuildProxy.FromBuild(build)
            };

            var result = await executor.TryExecuteAsync(data, CancellationToken.None);

            if (!result.Succeeded && result.Exception != null)
            {
                throw result.Exception;
            }
        }


        // Required boilerplate
        [FunctionName(Global.FunctionPrefix + nameof(BuildStatusChangedWatcherEntity))]
        public static Task Run(
            [EntityTrigger] IDurableEntityContext ctx,
            ILogger log
        ) => ctx.DispatchAsync<BuildStatusChangedWatcherEntity>(log);
    }
}