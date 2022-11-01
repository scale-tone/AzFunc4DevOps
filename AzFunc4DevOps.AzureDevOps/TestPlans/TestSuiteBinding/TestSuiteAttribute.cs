using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class TestSuiteAttribute : GenericProjectBindingAttribute
    {
        [AutoResolve]
        public string PlanId { get; set; }

        [AutoResolve]
        public string Id { get; set; }
    }
}