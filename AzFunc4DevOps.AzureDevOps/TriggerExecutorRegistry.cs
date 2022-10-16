using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host.Executors;

namespace AzFunc4DevOps.AzureDevOps
{
    public class TriggerExecutorRegistry
    {
        private class AttributeAndExecutor
        {
            public GenericTriggerAttribute TriggerAttribute { get; }
            public ITriggeredFunctionExecutor Executor { get; }

            public AttributeAndExecutor(GenericTriggerAttribute attribute, ITriggeredFunctionExecutor executor)
            {
                this.TriggerAttribute = attribute;
                this.Executor = executor;
            }
        }

        public void RegisterTriggerExecutor<TWatcherEntity>(GenericTriggerAttribute attribute, ITriggeredFunctionExecutor executor)
        {
            // Formatting an id of the entity that will take care about this trigger binding
            string entityTypeName = typeof(TWatcherEntity).Name;
            var entityId = new EntityId(Global.FunctionPrefix + entityTypeName, attribute.GetWatcherEntityKey());

            // Registering that entity in the map
            this._executorMap[entityId] = new AttributeAndExecutor(attribute, executor);
        }

        public ITriggeredFunctionExecutor GetExecutorForEntity(EntityId entityId)
        {
            if (this._executorMap.TryGetValue(entityId, out var attributeAndExecutor))
            {
                return attributeAndExecutor.Executor;
            }

            throw new ArgumentException($"Failed to find a trigger executor for entity {entityId}");
        }

        public GenericTriggerAttribute TryGetTriggerAttributeForEntity(EntityId entityId)
        {
            this._executorMap.TryGetValue(entityId, out var attributeAndExecutor);
            return attributeAndExecutor?.TriggerAttribute;
        }

        public List<EntityId> GetEntityIds()
        {
            return this._executorMap.Keys.ToList();
        }

        private ConcurrentDictionary<EntityId, AttributeAndExecutor> _executorMap = new ConcurrentDictionary<EntityId, AttributeAndExecutor>();
    }
}