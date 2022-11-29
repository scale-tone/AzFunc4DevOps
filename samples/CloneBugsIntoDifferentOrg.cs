using AzFunc4DevOps.AzureDevOps;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.Azure.WebJobs;

namespace AzFunc4DevOps.Samples
{
    public static class CloneBugsIntoDifferentOrg
    {
        /*
            Whenever a bug is created in Org1, creates its clone in Org2
        */
        [FunctionName(nameof(CloneBugsIntoDifferentOrg))]
        [return: WorkItem(
            OrgUrl = "%AZURE_DEVOPS_ORG_URL_2%",
            PersonalAccessToken = "%AZURE_DEVOPS_PAT_2%",
            Project = "%TEAM_PROJECT_NAME_2%"
        )]
        public static WorkItemProxy Run
        (
            // When a bug is created in Org1
            [WorkItemCreatedTrigger
            (
                OrgUrl = "%AZURE_DEVOPS_ORG_URL_1%",
                PersonalAccessToken = "%AZURE_DEVOPS_PAT_1%",
                Project = "%TEAM_PROJECT_NAME_1%", 
                WiqlQueryWhereClause = "[System.WorkItemType] = 'Bug'"
            )] 
            WorkItemProxy bug
        )
        {
            // Copying general fields
            var newBug = new WorkItemProxy
            {
                Priority = bug.Priority,
                Severity = bug.Severity,
                WorkItemType = bug.WorkItemType,
                Title = bug.Title,
                Description = bug.Description
            };

            // Also copying bug-specific `Repro Steps` field
            if (bug.Fields.TryGetValue("Microsoft.VSTS.TCM.ReproSteps", out var reproSteps))
            {
                newBug.Fields["Microsoft.VSTS.TCM.ReproSteps"] = reproSteps;
            }

            // Returning the new bug so that it gets created in Org2. Alternatively you could do it with an out parameter or ICollector/IAsyncCollector.
            return newBug;
        }
    }
}