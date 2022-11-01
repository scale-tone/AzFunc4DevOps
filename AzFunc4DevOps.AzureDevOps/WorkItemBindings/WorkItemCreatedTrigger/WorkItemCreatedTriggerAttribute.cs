using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    public class WorkItemCreatedTriggerAttribute : GenericTriggerAttribute
    {
        [AutoResolve]
        public string WiqlQueryWhereClause { get; set; }
    }
}