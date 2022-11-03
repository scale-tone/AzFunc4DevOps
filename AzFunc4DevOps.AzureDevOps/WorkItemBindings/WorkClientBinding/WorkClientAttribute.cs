using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Injects WorkHttpClient instance into your Function.
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.teamfoundation.work.webapi.workhttpclient"/>
    /// </summary>
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