using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
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