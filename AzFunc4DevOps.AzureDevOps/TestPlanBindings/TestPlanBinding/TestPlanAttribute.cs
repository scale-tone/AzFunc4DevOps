using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Binds to a <see cref="TestPlanProxy"/> (a collection of Test Cases).
    /// Input/Output.
    /// </summary>
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class TestPlanAttribute : GenericProjectBindingAttribute
    {
        /// <summary>
        /// ID of Test Plan to bind to. Integer number.
        /// Required.
        /// </summary>
        [AutoResolve]
        public string Id { get; set; }
    }
}