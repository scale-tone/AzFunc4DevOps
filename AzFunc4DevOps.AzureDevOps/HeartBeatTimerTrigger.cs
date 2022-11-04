using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AzFunc4DevOps.AzureDevOps
{
    public class HeartBeatTimerTrigger
    {
        public HeartBeatTimerTrigger(TriggerExecutorRegistry executorRegistry)
        {
            this._executorRegistry = executorRegistry;
        }

        private readonly TriggerExecutorRegistry _executorRegistry;
        public static readonly Regex EntityIdRegex = new Regex(@"@([\w-]+)@(.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal const int TimerIntervalInSec = 60;
        private const string TimerCronExpr = "0 * * * * *";

        [FunctionName(Global.FunctionPrefix + nameof(HeartBeatTimerTrigger))]
        public async Task Run
        (
            [TimerTrigger(TimerCronExpr)] TimerInfo timer,
            [DurableClient] IDurableClient durableClient
        )
        {
            // Time moment when all entities are expected to finish their current watch iteration.
            // Acts as a cancellation token.
            var whenToStop = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(TimerIntervalInSec - 1);

            var activeEntityIds = this._executorRegistry.GetEntityIds();

            // Just triggering all registered watcher entities
            var watchTasks = activeEntityIds
                .Select(entityId => durableClient
                    .SignalEntityAsync<IGenericWatcherEntity>(entityId, e => e.Watch(whenToStop)))
                .ToList();

            await Task.WhenAll(watchTasks);
        }
    }
}