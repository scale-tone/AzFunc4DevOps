using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AzFunc4DevOps.AzureDevOps
{
    public class CleanupTimerTrigger
    {
        public CleanupTimerTrigger(TriggerExecutorRegistry executorRegistry)
        {
            this._executorRegistry = executorRegistry;
        }

        private readonly TriggerExecutorRegistry _executorRegistry;
        public static readonly Regex EntityIdRegex = new Regex(@"@([\w-]+)@(.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Every hour
        private const string TimerCronExpr = "* 0 * * * *";

        /// <summary>
        /// Does the cleanup. Deletes entities that are not used anymore.
        /// </summary>
        [FunctionName(Global.FunctionPrefix + nameof(CleanupTimerTrigger))]
        public async Task Run
        (
            [TimerTrigger(TimerCronExpr)] TimerInfo timer,
            [DurableClient] IDurableClient durableClient
        )
        {
            var activeEntityIds = this._executorRegistry.GetEntityIds();

            // Querying for all entities with our prefix in their name. Default page size (100) is enough.
            var instancesResponse = await durableClient.ListInstancesAsync(new OrchestrationStatusQueryCondition 
                { 
                    InstanceIdPrefix = $"@{Global.FunctionPrefix.ToLower()}"
                }, 
                CancellationToken.None
            );

            // Getting entities that are not active anymore
            var inactiveEntityIds = instancesResponse.DurableOrchestrationState
                .Select(i => EntityIdRegex.Match(i.InstanceId))
                .Where(m => m.Success)
                .Select(m => new EntityId(m.Groups[1].Value, m.Groups[2].Value))
                .Where(id => !activeEntityIds.Contains(id))
                .ToList();
            
            foreach(var id in inactiveEntityIds)
            {
                var entityState = await durableClient.ReadEntityStateAsync<object>(id);

                if (entityState.EntityExists)
                {
                    // Signalling the entity to delete itself
                    await durableClient.SignalEntityAsync<IGenericWatcherEntity>(id, e => e.Delete());
                }
            }

            await durableClient.CleanEntityStorageAsync(true, true, CancellationToken.None);
        }
    }
}