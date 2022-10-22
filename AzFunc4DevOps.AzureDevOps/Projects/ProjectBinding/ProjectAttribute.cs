using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ProjectAttribute : Attribute
    {
        [AutoResolve]
        public string ProjectName { get; set; }
    }
}