using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    public class ReleaseEnvironmentStatusChangedWatcherEntity : IGenericWatcherEntity
    {
        #region Entity State

        public Dictionary<string, EnvironmentStatus> CurrentStatuses { get; set; }

        #endregion

        public ReleaseEnvironmentStatusChangedWatcherEntity(ILogger log, VssConnection connection, TriggerExecutorRegistry executorRegistry)
        {
            this._connection = connection;
            this._executorRegistry = executorRegistry;
            this._log = log;
        }

        public async Task Watch(DateTimeOffset whenToStop)
        {
            var attribute = (ReleaseEnvironmentStatusChangedTriggerAttribute)this._executorRegistry.TryGetTriggerAttributeForEntity(Entity.Current.EntityId);
            if (attribute == null)
            {
                return;
            }

            int? definitionId = null;
            if (!string.IsNullOrWhiteSpace(attribute.ReleaseDefinitionId))
            {
                definitionId = int.Parse(attribute.ReleaseDefinitionId);
            }

            // There's some complicated heuristics below, that detects cases when we 'miss' some particular release stage state
            // (when the stage runs too quickly).
            // This flag is to ensure that this heuristics doesn't cause the trigger to fire twice.
            var shouldBeTriggeredOnlyOnce = (!string.IsNullOrWhiteSpace(attribute.FromValue)) || (!string.IsNullOrWhiteSpace(attribute.ToValue));

            var releaseClient = await this._connection.GetClientAsync<ReleaseHttpClient>();

            // Storing here the items which function invocation failed for. So that they are only retried during next polling session.
            var failedIds = new HashSet<(int, int)>();
            while (true)
            {
                var stages = (await releaseClient.GetReleasesAsync(
                        project: attribute.Project,
                        definitionId: definitionId,
                        createdBy: attribute.CreatedBy,
                        path: attribute.ReleaseFolderPath,
                        expand: ReleaseExpands.Environments,
                        // By far just taking the first batch of latest releases. TODO: check what's the actual batch size and whether it is big enough.
                        queryOrder: ReleaseQueryOrder.Descending
                    ))
                    .SelectMany(r => r.Environments)
                    .Where(s => {

                        var isMatch = true;

                        if (!string.IsNullOrWhiteSpace(attribute.ReleaseEnvironmentId))
                        {
                            isMatch = isMatch && s.Id == int.Parse(attribute.ReleaseEnvironmentId);
                        }

                        if (!string.IsNullOrWhiteSpace(attribute.ReleaseEnvironmentName))
                        {
                            isMatch = isMatch && s.Name == attribute.ReleaseEnvironmentName;
                        }

                        return isMatch;
                    })
                    .Select(s => new { 
                        Ids = (s.ReleaseId, EnvironmentId: s.Id),
                        // Can't use value type as dictionary key in entity state due to serialization issues (DurableClient fails to deserialize the state then),
                        // so using a string instead.
                        StringIds = $"({s.ReleaseId}:{s.Id})",
                        Status = s.Status
                    })
                    .ToList();

                if (this.CurrentStatuses == null)
                {
                    // At first run just saving the current snapshot and quitting
                    this.CurrentStatuses = stages.ToDictionary(b => b.StringIds, b => b.Status);
                    return;
                }

                var newStatuses = new Dictionary<string, EnvironmentStatus>();

                foreach (var stage in stages)
                {
                    this.CurrentStatuses.TryGetValue(stage.StringIds, out var curStatus);
                    
                    bool alreadyTriggered = (curStatus & AlreadyTriggeredFlag) != 0;
                    var curStatusWithoutFlag = curStatus & (~AlreadyTriggeredFlag);

                    if (
                        stage.Status != curStatusWithoutFlag && 
                        !failedIds.Contains(stage.Ids)
                    )
                    {
                        // Checking if the stage was redeployed
                        if (
                            shouldBeTriggeredOnlyOnce &&
                            alreadyTriggered &&
                            curStatusWithoutFlag.IsMoreThan(stage.Status)
                        )
                        {
                            // Need to reset the status
                            curStatus = EnvironmentStatus.Undefined;
                            alreadyTriggered = false;
                        }

                        if (
                            (!alreadyTriggered) && 
                            this.CheckIfShouldBeTriggered(attribute, stage.Status)
                        )
                        {
                            try
                            {
                                // Intentionally using await, to distribute the load against Azure DevOps
                                await this.InvokeFunction(releaseClient, attribute.Project, stage.Ids.Item1, stage.Ids.Item2);

                                // Bumping up the known version and marking that function has already been triggered for this stage
                                curStatus = shouldBeTriggeredOnlyOnce ? (stage.Status | AlreadyTriggeredFlag) : stage.Status;
                            }
                            catch (Exception ex)
                            {
                                this._log.LogError(ex, $"ReleaseEnvironmentStatusChangedTrigger failed for release environment #({stage.StringIds})");

                                // Memorizing this item as failed, so that it is only retried next time.
                                failedIds.Add(stage.Ids);
                            }
                        }
                        else
                        {
                            // Just bumping up the known version
                            curStatus = alreadyTriggered ? (stage.Status | AlreadyTriggeredFlag) : stage.Status;
                        }
                    }

                    newStatuses[stage.StringIds] = curStatus;
                }

                // Setting new state
                this.CurrentStatuses = newStatuses;

                // Explicitly persisting current state
                Entity.Current.SetState(this);

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

        private static readonly EnvironmentStatus AlreadyTriggeredFlag = (EnvironmentStatus)0x40000000;

        private readonly VssConnection _connection;
        private readonly TriggerExecutorRegistry _executorRegistry;
        private readonly ILogger _log;

        private bool CheckIfShouldBeTriggered(ReleaseEnvironmentStatusChangedTriggerAttribute attr, EnvironmentStatus status)
        {
            bool isChanged = true;

            if (!string.IsNullOrWhiteSpace(attr.FromValue))
            {
                var fromStatus = (EnvironmentStatus)Enum.Parse(typeof(EnvironmentStatus), attr.FromValue);

                // Checking that current status is _more_ than fromStatus
                isChanged = isChanged && status.IsMoreThan(fromStatus);
            }

            if (!string.IsNullOrWhiteSpace(attr.ToValue))
            {
                var toStatus = (EnvironmentStatus)Enum.Parse(typeof(EnvironmentStatus), attr.ToValue);

                // Checking that current status is _more_or_equal_ than toStatus
                isChanged = isChanged && status.IsMoreThanOrEqual(toStatus);
            }

            return isChanged;
        }

        private async Task InvokeFunction(ReleaseHttpClient client, string projectName, int releaseId, int stageId)
        {
            var stage = await client.GetReleaseEnvironmentAsync(projectName, releaseId, stageId);

            var executor = this._executorRegistry.GetExecutorForEntity(Entity.Current.EntityId);

            var data = new TriggeredFunctionData
            {
                TriggerValue = ReleaseEnvironmentProxy.FromReleaseEnvironment(stage, client, projectName)
            };

            var result = await executor.TryExecuteAsync(data, CancellationToken.None);

            if (!result.Succeeded && result.Exception != null)
            {
                throw result.Exception;
            }
        }

        // Required boilerplate
        [FunctionName(Global.FunctionPrefix + nameof(ReleaseEnvironmentStatusChangedWatcherEntity))]
        public static Task Run(
            [EntityTrigger] IDurableEntityContext ctx,
            ILogger log
        ) => ctx.DispatchAsync<ReleaseEnvironmentStatusChangedWatcherEntity>(log);
    }

    internal static class ReleaseEnvironmentStatusChangedWatcherEntityStaticHelpers
    {
        public static bool IsMoreThan(this EnvironmentStatus firstStatus, EnvironmentStatus secondStatus)
        {
            switch (secondStatus)
            {
                case EnvironmentStatus.NotStarted:
                case EnvironmentStatus.Scheduled:
                    return firstStatus.In(EnvironmentStatus.Queued, EnvironmentStatus.InProgress, EnvironmentStatus.Succeeded, EnvironmentStatus.PartiallySucceeded, EnvironmentStatus.Canceled, EnvironmentStatus.Rejected);
                case EnvironmentStatus.Queued:
                    return firstStatus.In(EnvironmentStatus.InProgress, EnvironmentStatus.Succeeded, EnvironmentStatus.PartiallySucceeded, EnvironmentStatus.Canceled, EnvironmentStatus.Rejected);
                case EnvironmentStatus.InProgress:
                    return firstStatus.In(EnvironmentStatus.Succeeded, EnvironmentStatus.PartiallySucceeded, EnvironmentStatus.Canceled, EnvironmentStatus.Rejected);
            }

            return false;
        }

        public static bool IsMoreThanOrEqual(this EnvironmentStatus firstStatus, EnvironmentStatus secondStatus)
        {
            switch (secondStatus)
            {
                case EnvironmentStatus.NotStarted:
                    return firstStatus.In(EnvironmentStatus.NotStarted, EnvironmentStatus.Queued, EnvironmentStatus.InProgress, EnvironmentStatus.Succeeded, EnvironmentStatus.PartiallySucceeded, EnvironmentStatus.Canceled, EnvironmentStatus.Rejected);
                case EnvironmentStatus.Scheduled:
                    return firstStatus.In(EnvironmentStatus.Scheduled, EnvironmentStatus.Queued, EnvironmentStatus.InProgress, EnvironmentStatus.Succeeded, EnvironmentStatus.PartiallySucceeded, EnvironmentStatus.Canceled, EnvironmentStatus.Rejected);
                case EnvironmentStatus.Queued:
                    return firstStatus.In(EnvironmentStatus.Queued, EnvironmentStatus.InProgress, EnvironmentStatus.Succeeded, EnvironmentStatus.PartiallySucceeded, EnvironmentStatus.Canceled, EnvironmentStatus.Rejected);
                case EnvironmentStatus.InProgress:
                    return firstStatus.In(EnvironmentStatus.InProgress, EnvironmentStatus.Succeeded, EnvironmentStatus.PartiallySucceeded, EnvironmentStatus.Canceled, EnvironmentStatus.Rejected);
                case EnvironmentStatus.Succeeded:
                case EnvironmentStatus.PartiallySucceeded:
                case EnvironmentStatus.Canceled:
                case EnvironmentStatus.Rejected:
                    return firstStatus == secondStatus;
            }

            return false;
        }
    }
}