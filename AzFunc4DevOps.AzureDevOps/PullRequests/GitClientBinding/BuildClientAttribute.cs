using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class GitClientAttribute : Attribute
    {
        public static GitHttpClient CreateClient(VssConnection connection)
        {
            return connection.GetClient<GitHttpClient>();
        }
    }
}