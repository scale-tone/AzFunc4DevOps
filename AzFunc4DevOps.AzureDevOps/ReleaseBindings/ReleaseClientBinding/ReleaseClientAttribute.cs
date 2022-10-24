using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;

namespace AzFunc4DevOps.AzureDevOps 
{
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