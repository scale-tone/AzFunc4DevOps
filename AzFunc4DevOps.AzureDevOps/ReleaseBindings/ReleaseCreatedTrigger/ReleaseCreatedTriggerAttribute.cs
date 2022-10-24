using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    public class ReleaseCreatedTriggerAttribute : GenericTriggerAttribute
    {
        // TODO: rename to TeamProject (because it might be either name or id)
        [AutoResolve]
        public string ProjectName { get; set; }

        [AutoResolve]
        public string DefinitionId { get; set; }

        [AutoResolve]
        public string CreatedBy { get; set; }

        [AutoResolve]
        public string ReleaseFolderPath { get; set; }
    }
}