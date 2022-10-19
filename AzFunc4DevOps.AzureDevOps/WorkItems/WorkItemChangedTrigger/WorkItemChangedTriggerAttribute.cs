using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    public class WorkItemChangedTriggerAttribute : GenericTriggerAttribute
    {
        [AutoResolve]
        public string ProjectName { get; set; }

        [AutoResolve]
        public string WiqlQueryWhereClause { get; set; }

        [AutoResolve]
        public string FieldName { get; set; }

        [AutoResolve]
        public string FromValue { get; set; }

        [AutoResolve]
        public string ToValue { get; set; }
    }
}