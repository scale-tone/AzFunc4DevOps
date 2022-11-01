using System;
using Microsoft.Azure.WebJobs.Description;
using Newtonsoft.Json;

namespace AzFunc4DevOps.AzureDevOps 
{
    public abstract class GenericProjectBindingAttribute : Attribute
    {
        /// <summary>
        /// Team Project's name
        /// </summary>
        [AutoResolve]
        public string Project { get; set; }

        /// <summary>
        /// Need to redeem this field from being serialized
        /// </summary>
        [JsonIgnore]
        public override object TypeId => base.TypeId;
    }
}