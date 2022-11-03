using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Injects ReleaseHttpClient instance into your Function. 
    /// </summary>
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ReleaseClientAttribute : Attribute
    {
        public static ReleaseHttpClient CreateClient(VssConnection connection)
        {
            return connection.GetClient<ReleaseHttpClient>();
        }
    }
}