using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Binds to a <see cref="ProjectProxy"/> by its name.
    /// Input only.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ProjectAttribute : GenericProjectBindingAttribute
    {
    }
}