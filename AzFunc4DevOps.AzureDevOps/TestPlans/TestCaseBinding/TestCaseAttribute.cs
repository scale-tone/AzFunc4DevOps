using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Binds to a <see cref="TestCaseProxy"/> (a Work Item with Type = "Test Case").
    /// Input/Output.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class TestCaseAttribute : WorkItemAttribute
    {
        internal override Type GetSpecificWorkItemType()
        {
            return typeof(TestCaseProxy);
        }
    }
}