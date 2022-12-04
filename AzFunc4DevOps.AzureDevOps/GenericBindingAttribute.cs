using System;
using Microsoft.Azure.WebJobs.Description;
using Newtonsoft.Json;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Base class for all trigger and binding attributes
    /// </summary>
    public abstract class GenericBindingAttribute : Attribute
    {
        /// <summary>
        /// Azure DevOps Organization's URL.
        /// If specified, supersedes <see cref="Settings.AZFUNC4DEVOPS_AZURE_DEVOPS_ORG_URL"/> setting.
        /// </summary>
        [AutoResolve]
        public string OrgUrl { get; set; }

        /// <summary>
        /// Azure DevOps PAT (Personal Access Token).
        /// If specified, supersedes <see cref="Settings.AZFUNC4DEVOPS_AZURE_DEVOPS_PAT"/> setting.
        /// </summary>
        [AutoResolve]
        [JsonIgnore]
        public string PersonalAccessToken { get; set; }

        /// <summary>
        /// Need to redeem this field from being serialized
        /// </summary>
        [JsonIgnore]
        public override object TypeId => base.TypeId;
    }
}