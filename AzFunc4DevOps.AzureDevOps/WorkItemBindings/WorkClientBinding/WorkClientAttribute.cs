using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class WorkClientAttribute : Attribute
    {
        public static WorkHttpClient CreateClient(VssConnection connection)
        {
            return connection.GetClient<WorkHttpClient>();
        }
    }
}