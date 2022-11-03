using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Triggered when a new Release is created.
    /// </summary>
    [Binding]    
    public class ReleaseCreatedTriggerAttribute : GenericTriggerAttribute
    {
        /// <summary>
        /// When specified, only Releases from this Release Definition will be observed.
        /// </summary>
        [AutoResolve]
        public string ReleaseDefinitionId { get; set; }

        /// <summary>
        /// When specified, only Releases triggered by this user will be observed.
        /// E.g. <example>"john@doe.com"</example>
        /// </summary>
        [AutoResolve]
        public string CreatedBy { get; set; }

        /// <summary>
        /// When specified, only Releases from this folder will be observed.
        /// </summary>
        [AutoResolve]
        public string ReleaseFolderPath { get; set; }
    }
}