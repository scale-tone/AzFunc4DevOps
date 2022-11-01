using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    public class ReleaseCreatedTriggerAttribute : GenericTriggerAttribute
    {
        [AutoResolve]
        public string ReleaseDefinitionId { get; set; }

        [AutoResolve]
        public string CreatedBy { get; set; }

        [AutoResolve]
        public string ReleaseFolderPath { get; set; }
    }
}