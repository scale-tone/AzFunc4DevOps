using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;

namespace AzFunc4DevOps.AzureDevOps 
{
    public class GenericTriggerBinding<TWatcherEntity, TBindingValue> : ITriggerBinding
    {
        public GenericTriggerBinding(GenericTriggerAttribute attribute, TriggerExecutorRegistry executorRegistry) { 
            this._triggerAttribute = attribute;
            this._executorRegistry = executorRegistry;
        }

        public Type TriggerValueType => typeof(TBindingValue);

        public async Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            // Some plumming between this binding and triggered method's parameter
            return new TriggerData
            (
                new GenericValueProvider<TBindingValue>((TBindingValue)value),
                new Dictionary<string, object>()
            );
        }

        public async Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            // Registering this executor in our global executor registry
            this._executorRegistry.RegisterTriggerExecutor<TWatcherEntity>(this._triggerAttribute, context.Executor);

            // The below line doesn't do anything, actually
            return new GenericListener();
        }

        /// <summary>
        /// TODO: figure out what this property is for and when it is used
        /// </summary>
        public IReadOnlyDictionary<string, Type> BindingDataContract => new Dictionary<string, Type>();

        /// <summary>
        /// TODO: figure out what this method is for and when it is used
        /// </summary>
        public ParameterDescriptor ToParameterDescriptor()
        {
            return new TriggerParameterDescriptor()
            {
                Name = "AzFuncDevOpsTrigger"
            };
        }

        private readonly GenericTriggerAttribute _triggerAttribute;
        private readonly TriggerExecutorRegistry _executorRegistry;
    }
}
