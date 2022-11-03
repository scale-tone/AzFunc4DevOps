using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Injects WorkHttpClient instance into your Function.
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.teamfoundation.workitemtracking.webapi.workitemtrackinghttpclient"/>
    /// </summary>
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