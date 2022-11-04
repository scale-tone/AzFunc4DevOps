using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Triggers a Function when a Build status is changed.
    /// </summary>
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class BuildStatusChangedTriggerAttribute : GenericTriggerAttribute
    {
        /// <summary>
        /// Comma-separated list of Build Definition IDs.
        /// When specified, only Builds from those Build Definitions will be observed.
        /// </summary>
        [AutoResolve]
        public string BuildDefinitionIds { get; set; }

        /// <summary>
        /// Comma-separated list of Agent Pool IDs.
        /// When specified, only Builds running in those Agent Pools will be observed.
        /// </summary>
        [AutoResolve]
        public string AgentPoolIds { get; set; }

        /// <summary>
        /// When specified, only builds with that particular Build Number will be observed.
        /// E.g. <example>"20221102.5"</example>.
        /// </summary>
        [AutoResolve]
        public string BuildNumber { get; set; }

        /// <summary>
        /// When specified, only builds triggered by that particular person will be observed.
        /// E.g. <example>"john@doe.com"</example>.
        /// </summary>
        [AutoResolve]
        public string RequestedFor { get; set; }

        /// <summary>
        /// Comma-separated list of reasons for build to be triggered.
        /// When specified, only builds caused by those reasons will be observed.
        /// List of allowed values: <see href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.teamfoundation.build.webapi.buildreason" />.
        /// E.g. <example>"Manual,PullRequest,Schedule"</example>.
        /// </summary>
        [AutoResolve]
        public string BuildReasons { get; set; }

        /// <summary>
        /// When specified, only builds from that repository will be observed.
        /// E.g. <example>"14BAE3DC-067A-4A07-8BA9-1223646C9F44"</example>.
        /// TODO: needs to be used together with RepositoryType, so doesn't work yet. Replace with RepositoryName.
        /// </summary>
        [AutoResolve]
        internal string RepositoryId { get; set; }


        /// <summary>
        /// When specified, the Function will only be triggered when Build Status changes FROM this value.
        /// List of allowed values: <see href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.teamfoundation.build.webapi.buildstatus"/>.
        /// E.g. <example>"NotStarted"</example>.
        /// </summary>
        [AutoResolve]
        public string FromValue { get; set; }

        /// <summary>
        /// When specified, the Function will only be triggered when Build Status changes TO this value.
        /// List of allowed values: <see href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.teamfoundation.build.webapi.buildstatus"/>.
        /// E.g. <example>"Completed"</example>.
        /// </summary>
        [AutoResolve]
        public string ToValue { get; set; }
    }
}