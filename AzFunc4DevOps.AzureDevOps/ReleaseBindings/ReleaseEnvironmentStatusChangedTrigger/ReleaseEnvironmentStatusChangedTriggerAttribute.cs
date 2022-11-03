using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Triggered when a Release Environment (Stage) changes its Status.
    /// </summary>
    [Binding]    
    public class ReleaseEnvironmentStatusChangedTriggerAttribute : GenericTriggerAttribute
    {
        /// <summary>
        /// When specified, only Releases from this Release Definition will be observed.
        /// </summary>
        [AutoResolve]
        public string ReleaseDefinitionId { get; set; }

        /// <summary>
        /// When specified, only Releases created by this user will be observed.
        /// E.g. <example>"john@doe.com"</example>
        /// </summary>
        [AutoResolve]
        public string CreatedBy { get; set; }

        /// <summary>
        /// When specified, only Releases from this Folder will be observed.
        /// </summary>
        [AutoResolve]
        public string ReleaseFolderPath { get; set; }

        /// <summary>
        /// When specified, only this particular Release Environment (Stage) will be observed.
        /// </summary>
        [AutoResolve]
        public string ReleaseEnvironmentId { get; set; }

        /// <summary>
        /// When specified, only Release Environments (Stages) with this particular name will be observed.
        /// E.g. <example>"Stage 1"</example>
        /// </summary>
        [AutoResolve]
        public string ReleaseEnvironmentName { get; set; }

        /// <summary>
        /// When specified, the Function will only be triggered when Release Environment Status changes FROM this value.
        /// Allowed values: <see cref="Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.EnvironmentStatus"/>.
        /// E.g. <example>"NotStarted"</example>.
        /// </summary>
        [AutoResolve]
        public string FromValue { get; set; }

        /// <summary>
        /// When specified, the Function will only be triggered when Release Environment Status changes TO this value.
        /// Allowed values: <see cref="Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.EnvironmentStatus"/>.
        /// E.g. <example>"Succeeded"</example>.
        /// </summary>
        [AutoResolve]
        public string ToValue { get; set; }
    }
}