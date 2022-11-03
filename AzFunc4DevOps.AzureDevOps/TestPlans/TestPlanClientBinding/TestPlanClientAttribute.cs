using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Injects TestPlanHttpClient instance into your Function. 
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.services.testmanagement.testplanning.webapi.testplanhttpclient"/>
    /// </summary>
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class TestPlanClientAttribute : Attribute
    {
        public static TestPlanHttpClient CreateClient(VssConnection connection)
        {
            return connection.GetClient<TestPlanHttpClient>();
        }
    }
}