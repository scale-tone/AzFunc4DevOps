using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class WorkItemsAttribute : GenericProjectBindingAttribute
    {
        [AutoResolve]
        public string WiqlQueryWhereClause { get; set; }
    }
}