using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Triggers;

namespace AzFunc4DevOps.AzureDevOps 
{
    public class GenericTriggerBindingProvider<TAttribute, TBinding> : ITriggerBindingProvider
        where TAttribute: GenericTriggerAttribute
    {
        public GenericTriggerBindingProvider(TriggerExecutorRegistry executorRegistry, INameResolver nameResolver)
        {
            this._executorRegistry = executorRegistry;
            this._nameResolver = nameResolver;
        }

        public async Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            // Checking if it is our parameter (decorated with TAttribute)
            var attribute = context.Parameter.GetCustomAttribute<TAttribute>(false);
            if (attribute == null)
            {
                return null;
            }

            // Resolving attribute property values with DefaultNameResolver (which replaces %values% with config values).
            // This happens automatically for bindings, but for triggers for some reason it does not.
            // So we need to do that ourselves.
            foreach(var propInfo in AttributePropInfos)
            {
                string attributePropertyValue = (string)propInfo.GetValue(attribute);

                if (!string.IsNullOrWhiteSpace(attributePropertyValue))
                {
                    string resolvedAttributePropertyValue = this._nameResolver.ResolveWholeString(attributePropertyValue);

                    propInfo.SetValue(attribute, resolvedAttributePropertyValue);
                }
            }

            // Returning our binding
            return (ITriggerBinding)Activator.CreateInstance(typeof(TBinding), attribute, this._executorRegistry);
        }

        private readonly TriggerExecutorRegistry _executorRegistry;
        private readonly INameResolver _nameResolver;

        private static readonly PropertyInfo[] AttributePropInfos = typeof(TAttribute)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(string))
            .ToArray();
    }
}
