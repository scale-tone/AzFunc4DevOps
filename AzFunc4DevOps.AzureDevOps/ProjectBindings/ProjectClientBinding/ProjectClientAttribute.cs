using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.TeamFoundation.Core.WebApi;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Injects ProjectHttpClient instance into your Function. 
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.teamfoundation.core.webapi.projecthttpclient"/>
    /// </summary>
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ProjectClientAttribute : GenericBindingAttribute
    {
        /// <inheritdoc />
        internal ProjectHttpClient CreateClient(VssConnectionFactory connFactory)
        {
            return connFactory.GetVssConnection(this).GetClient<ProjectHttpClient>();
        }
    }
}