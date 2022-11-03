using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Binds to a <see cref="TestSuiteProxy"/> (a collection of Test Cases).
    /// Input/Output.
    /// </summary>
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