using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Binds to a <see cref="WorkItemProxy"/> (a generic Work Item instance).
    /// Input/Output.
    /// </summary>
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class WorkItemAttribute : GenericProjectBindingAttribute
    {
        /// <summary>
        /// ID of a Work Item to bind to. Should only be used for Input bindings.
        /// </summary>
        [AutoResolve]
        public string Id { get; set; }

        internal virtual Type GetSpecificWorkItemType()
        {
            return typeof(WorkItemProxy);
        }
    }
}