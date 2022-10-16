using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    public class WorkItemChangedTriggerAttribute : GenericTriggerAttribute
    {
        public string ProjectName { get; set; }
        public string WiqlQueryWhereClause { get; set; }

        public string FieldName { get; set; }
        public string FromValue { get; set; }
        public string ToValue { get; set; }

        public override string GetWatcherEntityKey()
        {
            //TODO: use binding validators instead
            if (string.IsNullOrWhiteSpace(this.ProjectName))
            {
                throw new ArgumentException("Non-empty ProjectName is required");
            }

            if (string.IsNullOrWhiteSpace(this.WiqlQueryWhereClause))
            {
                throw new ArgumentException("Non-empty WiqlQueryWhereClause is required");
            }

            return $"{this.ProjectName}-{this.WiqlQueryWhereClause}-{this.FieldName}-{this.FromValue}-{this.ToValue}".GetMd5Hash();
        }
    }
}