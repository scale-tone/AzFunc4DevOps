using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ProjectClientAttribute : Attribute
    {
        public static ProjectHttpClient CreateClient(VssConnection connection)
        {
            return connection.GetClient<ProjectHttpClient>();
        }
    }
}