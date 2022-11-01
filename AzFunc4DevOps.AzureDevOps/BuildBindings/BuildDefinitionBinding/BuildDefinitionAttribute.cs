using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class BuildDefinitionAttribute : GenericProjectBindingAttribute
    {
        [AutoResolve]
        public string Id { get; set; }
    }
}