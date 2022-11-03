using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Output-only binding for <see cref="ReleaseEnvironmentProxy"/> object.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class ReleaseEnvironmentAttribute : GenericProjectBindingAttribute
    {
    }
}