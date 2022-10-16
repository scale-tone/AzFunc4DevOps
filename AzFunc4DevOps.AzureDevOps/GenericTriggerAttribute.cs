using System;
using System.Security.Cryptography;
using System.Text;

namespace AzFunc4DevOps.AzureDevOps 
{

    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class GenericTriggerAttribute : Attribute
    {
        public abstract string GetWatcherEntityKey();
    }
}