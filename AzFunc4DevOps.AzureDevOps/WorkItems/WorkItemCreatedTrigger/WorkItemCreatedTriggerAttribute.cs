using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    public class WorkItemCreatedTriggerAttribute : GenericTriggerAttribute
    {
        public string ProjectName { get; set; }
        public string WiqlQueryWhereClause { get; set; }

        public override string GetWatcherEntityKey()
        {
            if (string.IsNullOrWhiteSpace(this.ProjectName))
            {
                throw new ArgumentException("Non-empty ProjectName is required");
            }

            return $"{this.ProjectName}-{this.WiqlQueryWhereClause}".GetMd5Hash();
        }
    }
}