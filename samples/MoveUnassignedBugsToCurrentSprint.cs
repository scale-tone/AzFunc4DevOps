using AzFunc4DevOps.AzureDevOps;
using Microsoft.Azure.WebJobs;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsTestFunc
{
    public static class MoveUnassignedBugsToCurrentSprint
    {
        /*
            Periodically collects all unassigned bugs and moves them into current iteration    
        */
        [FunctionName(nameof(MoveUnassignedBugsToCurrentSprint))]
        public static async Task Run
        (
            // Every midnight
            [TimerTrigger("* * 0 * * *")] TimerInfo timer,

            // Bind to a collection of unassigned bugs
            [WorkItems(
                Project = "%TEAM_PROJECT_NAME%", 
                WiqlQueryWhereClause = "[System.WorkItemType] = 'Bug' AND [System.AssignedTo] = ''"
            )]
            IEnumerable<WorkItemProxy> unassignedBugs,

            // Will put updated bugs into this IAsyncCollector, so that changes are persisted
            [WorkItem(Project = "%TEAM_PROJECT_NAME%")]
            IAsyncCollector<WorkItemProxy> results,

            // To pick up current iteration
            [WorkClient]
            WorkHttpClient workClient
        )
        {
            // Determining the current iteration
            var currentIteration = (await workClient.GetTeamIterationsAsync
            (
                new TeamContext(Environment.GetEnvironmentVariable("TEAM_PROJECT_NAME")),
                "current"
            ))
            .FirstOrDefault();

            // Iterating through bugs
            foreach(var bug in unassignedBugs)
            {
                bug.IterationPath = currentIteration?.Path;

                // Saving changes, if any (WorkItemProxy tracks changes to itself, so this line will be a noop, if there were no actual changes)
                await results.AddAsync(bug);
            }
        }
    }
}
