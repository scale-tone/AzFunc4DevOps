using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Injects GitHttpClient instance into your Function.
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.teamfoundation.sourcecontrol.webapi.githttpclient"/>
    /// </summary>
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class GitClientAttribute : GenericBindingAttribute
    {
        /// <inheritdoc />
        public GitHttpClient CreateClient(VssConnectionFactory connFactory)
        {
            return connFactory.GetVssConnection(this).GetClient<GitHttpClient>();
        }
    }
}