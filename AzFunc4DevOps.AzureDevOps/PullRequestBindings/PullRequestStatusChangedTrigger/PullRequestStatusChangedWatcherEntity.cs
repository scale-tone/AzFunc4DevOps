using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    public class PullRequestStatusChangedWatcherEntity : IGenericWatcherEntity
    {
        #region Entity State

        public Dictionary<int, PullRequestStatusEnum> CurrentStatuses { get; set; }

        #endregion

        public PullRequestStatusChangedWatcherEntity(ILogger log, VssConnectionFactory connFactory, TriggerExecutorRegistry executorRegistry)
        {
            this._connFactory = connFactory;
            this._executorRegistry = executorRegistry;
            this._log = log;
        }

        public async Task Watch(DateTimeOffset whenToStop)
        {
            var attribute = (PullRequestStatusChangedTriggerAttribute)this._executorRegistry.TryGetTriggerAttributeForEntity(Entity.Current.EntityId);
            if (attribute == null)
            {
                return;
            }

            var searchCriteria = new GitPullRequestSearchCriteria
            {
                SourceRefName = this.FixBranchName(attribute.SourceBranch),
                TargetRefName = this.FixBranchName(attribute.TargetBranch),
                Status = PullRequestStatus.All
            };

            var client = await this._connFactory.GetVssConnection(attribute).GetClientAsync<GitHttpClient>();

            // Storing here the items which function invocation failed for. So that they are only retried during next polling session.
            var failedIds = new HashSet<int>();
            while (true)
            {
                var pullRequests = (await client.GetPullRequestsByProjectAsync(attribute.Project, searchCriteria))
                    .Where(pr => string.IsNullOrWhiteSpace(attribute.Repository) ? 
                        true : 
                        pr.Repository.Name == attribute.Repository)
                    .ToList();

                if (this.CurrentStatuses == null)
                {
                    // At first run just saving the current snapshot and quitting
                    this.CurrentStatuses = pullRequests.ToDictionary(b => b.PullRequestId, b => this.ConvertStatus(b) );
                    return;
                }

                var newStatuses = new Dictionary<int, PullRequestStatusEnum>();

                foreach (var pullRequest in pullRequests)
                {
                    var newStatus = this.ConvertStatus(pullRequest);
                    this.CurrentStatuses.TryGetValue(pullRequest.PullRequestId, out var curStatus);

                    if (newStatus != curStatus && !failedIds.Contains(pullRequest.PullRequestId))
                    {
                        try
                        {
                            if (this.CheckIfShouldBeTriggered(attribute, curStatus, newStatus))
                            {
                                // Intentionally using await, to distribute the load against Azure DevOps
                                await this.InvokeFunction(pullRequest);
                            }

                            // Bumping up the known version
                            curStatus = newStatus;
                        }
                        catch (Exception ex)
                        {                            
                            this._log.LogError(ex, $"PullRequestStatusChangedTrigger failed for pull request #{pullRequest.PullRequestId}");

                            // Memorizing this item as failed, so that it is only retried next time.
                            failedIds.Add(pullRequest.PullRequestId);
                        }
                    }

                    newStatuses[pullRequest.PullRequestId] = curStatus;
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

        private readonly VssConnectionFactory _connFactory;
        private readonly TriggerExecutorRegistry _executorRegistry;
        private readonly ILogger _log;

        private bool CheckIfShouldBeTriggered(PullRequestStatusChangedTriggerAttribute attr, PullRequestStatusEnum oldStatus, PullRequestStatusEnum newStatus)
        {
            bool isChanged = true;

            if (!string.IsNullOrWhiteSpace(attr.FromValue))
            {
                var fromStatus = (PullRequestStatusEnum)Enum.Parse(typeof(PullRequestStatusEnum), attr.FromValue);

                isChanged = isChanged && (oldStatus == fromStatus);
            }

            if (!string.IsNullOrWhiteSpace(attr.ToValue))
            {
                var toStatus = (PullRequestStatusEnum)Enum.Parse(typeof(PullRequestStatusEnum), attr.ToValue);

                isChanged = isChanged && (newStatus == toStatus);
            }
            
            return isChanged;
        }

        private async Task InvokeFunction(GitPullRequest pullRequest)
        {
            var executor = this._executorRegistry.GetExecutorForEntity(Entity.Current.EntityId);

            var data = new TriggeredFunctionData()
            {
                TriggerValue = PullRequestProxy.FromPullRequest(pullRequest)
            };

            var result = await executor.TryExecuteAsync(data, CancellationToken.None);

            if (!result.Succeeded && result.Exception != null)
            {
                throw result.Exception;
            }
        }

        private string FixBranchName(string branchName)
        {
            if (string.IsNullOrWhiteSpace(branchName))
            {
                return null;
            }

            if (branchName.Contains('/'))
            {
                return branchName;
            }

            return $"refs/heads/{branchName}";
        }

        private PullRequestStatusEnum ConvertStatus(GitPullRequest pullRequest)
        {
            if (pullRequest.IsDraft == true)
            {
                return PullRequestStatusEnum.Draft;
            }

            return (PullRequestStatusEnum)pullRequest.Status;
        }


        // Required boilerplate
        [FunctionName(Global.FunctionPrefix + nameof(PullRequestStatusChangedWatcherEntity))]
        public static Task Run(
            [EntityTrigger] IDurableEntityContext ctx,
            ILogger log
        ) => ctx.DispatchAsync<PullRequestStatusChangedWatcherEntity>(log);
    }
}