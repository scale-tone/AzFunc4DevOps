using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Injects VssConnection instance into your Function. 
    /// </summary>
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class VssConnectionAttribute : Attribute
    {
    }
}