using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Binds to a <see cref="BuildDefinitionProxy"/> by its ID.
    /// Input only.
    /// </summary>
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class BuildDefinitionAttribute : GenericProjectBindingAttribute
    {
        /// <summary>
        /// Build Definition ID (an integer number) to bind to.
        /// </summary>
        [AutoResolve]
        public string Id { get; set; }
    }
}