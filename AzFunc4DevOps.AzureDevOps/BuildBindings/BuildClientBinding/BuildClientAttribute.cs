using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class BuildClientAttribute : Attribute
    {
        public static BuildHttpClient CreateClient(VssConnection connection)
        {
            return connection.GetClient<BuildHttpClient>();
        }
    }
}