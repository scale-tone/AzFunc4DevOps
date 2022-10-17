using System;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Newtonsoft.Json.Linq;

namespace AzFunc4DevOps.AzureDevOps 
{
    public class WorkItemProxy : WorkItem 
    {
        #region Field Definitions

        public long? AreaId 
        {
            get 
            {
                this.Fields.TryGetValue("System.AreaId", out var val);
                return val == null ? null : (long?)val;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("System.AreaId");
                }
                else 
                {
                    this.Fields["System.AreaId"] = value.Value;
                }
            }
        }

        public string AreaPath
        {
            get 
            {
                this.Fields.TryGetValue("System.AreaPath", out var val);
                return val as string;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("System.AreaPath");
                }
                else 
                {
                    this.Fields["System.AreaPath"] = value;
                }
            }
        }

        public string TeamProject
        {
            get 
            {
                this.Fields.TryGetValue("System.TeamProject", out var val);
                return val as string;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("System.TeamProject");
                }
                else 
                {
                    this.Fields["System.TeamProject"] = value;
                }
            }
        }

        public DateTime? AuthorizedDate
        {
            get 
            {
                this.Fields.TryGetValue("System.AuthorizedDate", out var val);
                return val == null ? null : (DateTime?)val;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("System.AuthorizedDate");
                }
                else 
                {
                    this.Fields["System.AuthorizedDate"] = value.Value;
                }
            }
        }

        public DateTime? RevisedDate
        {
            get 
            {
                this.Fields.TryGetValue("System.RevisedDate", out var val);
                return val == null ? null : (DateTime?)val;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("System.RevisedDate");
                }
                else 
                {
                    this.Fields["System.RevisedDate"] = value.Value;
                }
            }
        }

        public long? IterationId 
        {
            get 
            {
                this.Fields.TryGetValue("System.IterationId", out var val);
                return val == null ? null : (long?)val;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("System.IterationId");
                }
                else 
                {
                    this.Fields["System.IterationId"] = value.Value;
                }
            }
        }

        public string IterationPath 
        {
            get 
            {
                this.Fields.TryGetValue("System.IterationPath", out var val);
                return val as string;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("System.IterationPath");
                }
                else 
                {
                    this.Fields["System.IterationPath"] = value;
                }
            }
        }

        public string WorkItemType 
        {
            get 
            {
                this.Fields.TryGetValue("System.WorkItemType", out var val);
                return val as string;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("System.WorkItemType");
                }
                else 
                {
                    this.Fields["System.WorkItemType"] = value;
                }
            }
        }

        public string State 
        {
            get 
            {
                this.Fields.TryGetValue("System.State", out var val);
                return val as string;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("System.State");
                }
                else 
                {
                    this.Fields["System.State"] = value;
                }
            }
        }

        public string Reason 
        {
            get 
            {
                this.Fields.TryGetValue("System.Reason", out var val);
                return val as string;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("System.Reason");
                }
                else 
                {
                    this.Fields["System.Reason"] = value;
                }
            }
        }

        public DateTime? CreatedDate
        {
            get 
            {
                this.Fields.TryGetValue("System.CreatedDate", out var val);
                return val == null ? null : (DateTime?)val;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("System.CreatedDate");
                }
                else 
                {
                    this.Fields["System.CreatedDate"] = value.Value;
                }
            }
        }

        public IdentityRef CreatedBy 
        {
            get 
            {
                this.Fields.TryGetValue("System.CreatedBy", out var val);
                return val as IdentityRef;
            }
        }

        public DateTime? ChangedDate
        {
            get 
            {
                this.Fields.TryGetValue("System.ChangedDate", out var val);
                return val == null ? null : (DateTime?)val;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("System.ChangedDate");
                }
                else 
                {
                    this.Fields["System.ChangedDate"] = value.Value;
                }
            }
        }

        public IdentityRef ChangedBy 
        {
            get 
            {
                this.Fields.TryGetValue("System.ChangedBy", out var val);
                return val as IdentityRef;
            }
        }

        public long? Priority 
        {
            get 
            {
                this.Fields.TryGetValue("Microsoft.VSTS.Common.Priority", out var val);
                return val == null ? null : (long?)val;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("Microsoft.VSTS.Common.Priority");
                }
                else 
                {
                    this.Fields["Microsoft.VSTS.Common.Priority"] = value.Value;
                }
            }
        }

        public string Severity 
        {
            get 
            {
                this.Fields.TryGetValue("Microsoft.VSTS.Common.Severity", out var val);
                return val as string;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("Microsoft.VSTS.Common.Severity");
                }
                else 
                {
                    this.Fields["Microsoft.VSTS.Common.Severity"] = value;
                }
            }
        }

        public double? Effort 
        {
            get 
            {
                this.Fields.TryGetValue("Microsoft.VSTS.Scheduling.Effort", out var val);
                return val == null ? null : (double?)val;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("Microsoft.VSTS.Scheduling.Effort");
                }
                else 
                {
                    this.Fields["Microsoft.VSTS.Scheduling.Effort"] = value.Value;
                }
            }
        }

        public string Title 
        {
            get 
            {
                this.Fields.TryGetValue("System.Title", out var val);
                return val as string;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("System.Title");
                }
                else 
                {
                    this.Fields["System.Title"] = value;
                }
            }
        }

        public string Description 
        {
            get 
            {
                this.Fields.TryGetValue("System.Description", out var val);
                return val as string;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("System.Description");
                }
                else 
                {
                    this.Fields["System.Description"] = value;
                }
            }
        }
        
        public long? Parent 
        {
            get 
            {
                this.Fields.TryGetValue("System.Parent", out var val);
                return val == null ? null : (long?)val;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("System.Parent");
                }
                else 
                {
                    this.Fields["System.Parent"] = value.Value;
                }
            }
        }

        #endregion

        public JObject OriginalJson { get; private set; }

        internal static WorkItemProxy FromWorkItem(WorkItem item)
        {
            var jObject = JObject.FromObject(item);
            var proxy = jObject.ToObject<WorkItemProxy>();

            // Preserving the original values, to be able to detect changes later
            proxy.OriginalJson = jObject;

            return proxy;
        }

        public JsonPatchDocument GetJsonPatchDocument()
        {
            var doc = new JsonPatchDocument();

            var original = this.OriginalJson == null ? new WorkItem() : this.OriginalJson.ToObject<WorkItem>();

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

            if (doc.Count > 0 && original.Rev.HasValue)
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