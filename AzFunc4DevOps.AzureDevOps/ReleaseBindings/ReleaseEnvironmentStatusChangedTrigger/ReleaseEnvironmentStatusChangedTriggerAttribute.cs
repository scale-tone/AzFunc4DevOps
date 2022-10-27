using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    public class ReleaseEnvironmentStatusChangedTriggerAttribute : GenericTriggerAttribute
    {
        [AutoResolve]
        public string ProjectName { get; set; }

        [AutoResolve]
        public string ReleaseDefinitionId { get; set; }

        [AutoResolve]
        public string CreatedBy { get; set; }

        [AutoResolve]
        public string ReleaseFolderPath { get; set; }

        [AutoResolve]
        public string ReleaseEnvironmentId { get; set; }

        [AutoResolve]
        public string ReleaseEnvironmentName { get; set; }


        [AutoResolve]
        public string FromValue { get; set; }

        [AutoResolve]
        public string ToValue { get; set; }
    }
}