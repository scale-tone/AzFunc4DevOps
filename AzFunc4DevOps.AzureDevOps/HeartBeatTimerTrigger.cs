using System;
using System.Linq;
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

        private const int TimerIntervalInSec = 30;
        private const string TimerCronExpr = "*/30 * * * * *";

        private readonly TriggerExecutorRegistry _executorRegistry;

        private static DateTimeOffset LastTimeExecuted = DateTimeOffset.UtcNow;

        [FunctionName(Global.FunctionPrefix + nameof(HeartBeatTimerTrigger))]
        public async Task Run
        (
            [TimerTrigger(TimerCronExpr)] TimerInfo timer,
            [DurableClient] IDurableEntityClient durableClient
        )
        {
            // Time moment when all entities are expected to finish their current watch iteration.
            // Acts as a cancellation token.
            var whenToStop = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(TimerIntervalInSec - 1);

            var signalParams = new GenericWatcherEntityParams { LastTimeExecuted = LastTimeExecuted, WhenToStop = whenToStop };

            // Just triggering all registered watcher entities
            var tasks = this._executorRegistry.GetEntityIds()
                .Select(entityId => durableClient
                    .SignalEntityAsync<IGenericWatcherEntity<GenericWatcherEntityParams>>(entityId, e => e.Watch(signalParams)))
                .ToList();

            await Task.WhenAll(tasks);

            LastTimeExecuted = DateTimeOffset.UtcNow;
        }
    }
}