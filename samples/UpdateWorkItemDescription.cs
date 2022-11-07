using AzFunc4DevOps.AzureDevOps;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using System.IO;

namespace AzFunc4DevOps.Samples
{
    public static class UpdateWorkItemDescription
    {
        /*
            Updates Description field of a given workItem, identified by its workItemId.
            The new Description value comes from request body.
        */
        [FunctionName(nameof(UpdateWorkItemDescription))]
        [return: WorkItem(Project = "%TEAM_PROJECT_NAME%")]
        public static WorkItemProxy Run(
            [HttpTrigger(AuthorizationLevel.Function, "put")] HttpRequest req,
            
            // Binding to a WorkItem by workItemId, which is expected to come as a query string parameter
            [WorkItem(Project = "%TEAM_PROJECT_NAME%", Id = "{Query.workItemId}")]
            WorkItemProxy item
        )
        {
            // Setting item's Description field to request's body
            using var reader = new StreamReader(req.Body);

            item.Description = reader.ReadToEnd();

            // Returning the updated item, so that changes are persisted. Alternatively you could do it with an out parameter or ICollector/IAsyncCollector.
            return item;
        }
    }
}
