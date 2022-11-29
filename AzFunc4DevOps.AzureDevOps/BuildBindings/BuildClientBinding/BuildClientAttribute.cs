using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Injects BuildHttpClient instance into your Function. 
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.teamfoundation.build.webapi.buildhttpclient"/>
    /// </summary>
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class BuildClientAttribute : GenericBindingAttribute
    {
        internal BuildHttpClient CreateClient(VssConnectionFactory connFactory)
        {
            return connFactory.GetVssConnection(this).GetClient<BuildHttpClient>();
        }
    }
}