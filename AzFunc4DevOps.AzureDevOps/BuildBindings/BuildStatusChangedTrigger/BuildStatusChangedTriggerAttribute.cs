using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.TeamFoundation.Build.WebApi;
using Newtonsoft.Json;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class BuildStatusChangedTriggerAttribute : GenericTriggerAttribute
    {
        [AutoResolve]
        public string BuildDefinitionIds { get; set; }

        [AutoResolve]
        public string AgentPoolIds { get; set; }

        [AutoResolve]
        public string BuildNumber { get; set; }

        [AutoResolve]
        public string RequestedFor { get; set; }

        [AutoResolve]
        public string BuildReasons { get; set; }

        [AutoResolve]
        public string RepositoryId { get; set; }


        [AutoResolve]
        public string FromValue { get; set; }

        [AutoResolve]
        public string ToValue { get; set; }
    }
}