using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class WorkItemAttribute : Attribute
    {
        [AutoResolve]
        public string ProjectName { get; set; }

        [AutoResolve]
        public string WorkItemId { get; set; }
        
        [AutoResolve]
        public string WorkItemType { get; set; }
    }
}