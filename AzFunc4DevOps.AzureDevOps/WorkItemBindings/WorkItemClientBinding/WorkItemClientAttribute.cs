using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Injects WorkHttpClient instance into your Function.
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.teamfoundation.workitemtracking.webapi.workitemtrackinghttpclient"/>
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class WorkItemClientAttribute : GenericBindingAttribute
    {
        /// <inheritdoc />
        internal WorkItemTrackingHttpClient CreateClient(VssConnectionFactory connFactory)
        {
            return connFactory.GetVssConnection(this).GetClient<WorkItemTrackingHttpClient>();
        }
    }
}