using System;
using Newtonsoft.Json;

namespace AzFunc4DevOps.AzureDevOps 
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class GenericTriggerAttribute : GenericProjectBindingAttribute
    {
        public virtual string GetWatcherEntityKey()
        {
            // By default using this instance's JSON representation as an entity key
            return JsonConvert.SerializeObject(this).GetMd5Hash();
        }
    }
}