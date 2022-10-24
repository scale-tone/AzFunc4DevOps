using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    public class PullRequestStatusChangedTriggerAttribute : GenericTriggerAttribute
    {
        [AutoResolve]
        public string ProjectName { get; set; }

        [AutoResolve]
        public string Repository { get; set; }

        [AutoResolve]
        public string SourceBranch { get; set; }

        [AutoResolve]
        public string TargetBranch { get; set; }


        [AutoResolve]
        public string FromValue { get; set; }

        [AutoResolve]
        public string ToValue { get; set; }
    }
}