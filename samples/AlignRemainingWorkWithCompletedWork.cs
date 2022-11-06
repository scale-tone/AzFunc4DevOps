using AzFunc4DevOps.AzureDevOps;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.Azure.WebJobs;

namespace AzFunc4DevOps.Samples
{
    public static class AlignRemainingWorkWithCompletedWork
    {
        /*
            Whenever Task's CompletedWork field changes, updates its RemainingWork field accordingly
        */
        [FunctionName(nameof(AlignRemainingWorkWithCompletedWork))]
        [return: WorkItem(Project = "%TEAM_PROJECT_NAME%")]
        public static WorkItemProxy Run
        (
            // When CompletedWork value changes in a Task
            [WorkItemChangedTrigger
            (
                Project = "%TEAM_PROJECT_NAME%", 
                WiqlQueryWhereClause = "[System.WorkItemType] = 'Task'",
                FieldName = "Microsoft.VSTS.Scheduling.CompletedWork"
            )] 
            WorkItemChange change
        )
        {
            // change.NewVersion contains WorkItem's recent version. change.OldVersion contains previous version.
            var item = change.NewVersion;

            item.RemainingWork = item.OriginalEstimate - item.CompletedWork;

            // Returning the updated item, so that changes are persisted. Alternatively you could do it with an out parameter or ICollector/IAsyncCollector.
            return item;
        }
    }
}