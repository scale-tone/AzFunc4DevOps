using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class TestSuiteAttribute : Attribute
    {
        [AutoResolve]
        public string ProjectName { get; set; }

        [AutoResolve]
        public string PlanId { get; set; }

        [AutoResolve]
        public string Id { get; set; }
    }
}