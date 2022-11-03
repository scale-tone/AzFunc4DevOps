using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Injects ProjectHttpClient instance into your Function. 
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.teamfoundation.core.webapi.projecthttpclient"/>
    /// </summary>
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ProjectClientAttribute : Attribute
    {
        internal static ProjectHttpClient CreateClient(VssConnection connection)
        {
            return connection.GetClient<ProjectHttpClient>();
        }
    }
}