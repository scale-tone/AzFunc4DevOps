using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Triggers;

namespace AzFunc4DevOps.AzureDevOps 
{
    public class GenericTriggerBindingProvider<TAttribute, TBinding> : ITriggerBindingProvider
        where TAttribute: GenericTriggerAttribute
    {
        public GenericTriggerBindingProvider(TriggerExecutorRegistry executorRegistry)
        {
            this._executorRegistry = executorRegistry;
        }

        public async Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            // Checking if it is our parameter (decorated with TAttribute)
            var attribute = context.Parameter.GetCustomAttribute<TAttribute>(false);
            if (attribute == null)
            {
                return null;
            }

            // Returning our binding
            return (ITriggerBinding)Activator.CreateInstance(typeof(TBinding), attribute, this._executorRegistry);
        }

        private readonly TriggerExecutorRegistry _executorRegistry;
    }
}
