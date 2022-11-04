using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Triggered when a Pull Request changes its Status.
    /// Currently retries every 1 minute FOREVER. Make sure to handle your exceptions.
    /// </summary>
    [Binding]    
    public class PullRequestStatusChangedTriggerAttribute : GenericTriggerAttribute
    {
        /// <summary>
        /// When specified, only Pull Requests in this Repository will be observed.
        /// </summary>
        [AutoResolve]
        public string Repository { get; set; }

        /// <summary>
        /// When specified, only Pull Requests from this Source Branch will be observed.
        /// </summary>
        [AutoResolve]
        public string SourceBranch { get; set; }

        /// <summary>
        /// When specified, only Pull Requests into this Target Branch will be observed.
        /// </summary>
        [AutoResolve]
        public string TargetBranch { get; set; }

        /// <summary>
        /// When specified, the Function will only be triggered when Pull Request Status changes FROM this value.
        /// Allowed values: <see cref="PullRequestStatusEnum"/>.
        /// E.g. <example>"Draft"</example>.
        /// </summary>
        [AutoResolve]
        public string FromValue { get; set; }

        /// <summary>
        /// When specified, the Function will only be triggered when Pull Request Status changes TO this value.
        /// Allowed values: <see cref="PullRequestStatusEnum"/>.
        /// E.g. <example>"Completed"</example>.
        /// </summary>
        [AutoResolve]
        public string ToValue { get; set; }
    }
}