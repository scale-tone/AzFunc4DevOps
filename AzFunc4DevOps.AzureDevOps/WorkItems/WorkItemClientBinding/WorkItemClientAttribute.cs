using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class WorkItemClientAttribute : Attribute
    {
        public static WorkItemTrackingHttpClient CreateClient(VssConnection connection)
        {
            return connection.GetClient<WorkItemTrackingHttpClient>();
        }
    }
}