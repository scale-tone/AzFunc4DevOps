using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Output-only binding for <see cref="ReleaseEnvironmentStatusProxy"/> object.
    /// Used mostly for triggering Release Environments (Stages) and setting their Variables.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class ReleaseEnvironmentStatusAttribute : GenericProjectBindingAttribute
    {
    }
}