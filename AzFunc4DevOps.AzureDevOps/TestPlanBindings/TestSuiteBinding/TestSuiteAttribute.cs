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
        /// <summary>
        /// ID of parent Test Plan. Integer number.
        /// Required.
        /// </summary>
        [AutoResolve]
        public string PlanId { get; set; }

        /// <summary>
        /// (Optional) ID of a particular Test Suite.
        /// If not specified, binds to parent Test Plan's Root (aka default) Test Suite.
        /// </summary>
        [AutoResolve]
        public string Id { get; set; }
    }
}