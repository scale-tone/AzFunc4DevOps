using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Newtonsoft.Json.Linq;

namespace AzFunc4DevOps.AzureDevOps 
{
    public class WorkItemProxy : WorkItem 
    {
        public string Title 
        {
            get 
            {
                this.Fields.TryGetValue("System.Title", out var val);
                return val as string;
            }
            set
            {
                this.Fields["System.Title"] = value;
            }
        }

        public JObject Original { get; private set; }

        internal static WorkItemProxy FromWorkItem(WorkItem item)
        {
            var jObject = JObject.FromObject(item);
            var proxy = jObject.ToObject<WorkItemProxy>();

            // Preserving the original values, to be able to detect changes later
            proxy.Original = jObject;

            return proxy;
        }

        public JsonPatchDocument GetJsonPatchDocument(bool withOptimisticLock = true)
        {
            var doc = new JsonPatchDocument();

            var original = this.Original == null ? new WorkItem() : this.Original.ToObject<WorkItem>();

            foreach(var kv in this.Fields)
            {               
                if (original.Fields.ContainsKey(kv.Key))
                {
                    var originalValue = original.Fields[kv.Key];

                    if (this.AreDifferent(originalValue, kv.Value))
                    {
                        doc.Add(new JsonPatchOperation()
                        {
                            Operation = Operation.Replace,
                            Path = $"/fields/{kv.Key}",
                            Value = kv.Value
                        });
                    }

                    original.Fields.Remove(kv.Key);
                }
                else
                {
                    doc.Add(new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = $"/fields/{kv.Key}",
                        Value = kv.Value
                    });
                }
            }

            foreach(var deletedFieldName in original.Fields.Keys)
            {
                doc.Add(new JsonPatchOperation()
                {
                    Operation = Operation.Remove,
                    Path = $"/fields/{deletedFieldName}"
                });
            }

            if (doc.Count > 0 && original.Rev.HasValue && withOptimisticLock)
            {
                // Adding optimistic locking
                doc.Add(new JsonPatchOperation()
                {
                    Operation = Operation.Test,
                    Path = "/rev",
                    Value = original.Rev
                });
            }

            return doc;
        }

        internal bool CreatedByValueProvider;

        private bool AreDifferent(object oldValue, object newValue)
        {
            if (oldValue == null && newValue == null)
            {
                return false;
            }

            var type = oldValue == null ? newValue.GetType() : oldValue.GetType();

            // Right now only values and strings are supported (complex types are not)
            if (!(type.IsValueType || type == typeof(string)))
            {
                return false;
            }

            return oldValue == null ? true : !oldValue.Equals(newValue);
        }
    }
}