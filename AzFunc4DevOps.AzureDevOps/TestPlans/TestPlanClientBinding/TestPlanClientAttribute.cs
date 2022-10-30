using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps 
{
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