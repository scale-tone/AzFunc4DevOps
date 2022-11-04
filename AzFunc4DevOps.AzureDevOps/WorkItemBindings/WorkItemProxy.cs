using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Newtonsoft.Json.Linq;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Represents a generic Work Item.
    /// </summary>
    public class WorkItemProxy : WorkItem 
    {
        #region Field Definitions

        /// <summary>
        /// System.AreaId field.
        /// </summary>
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

        /// <summary>
        /// System.AreaPath field.
        /// </summary>
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

        /// <summary>
        /// System.TeamProject field.
        /// </summary>
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

        /// <summary>
        /// System.AuthorizedDate field.
        /// </summary>
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

        /// <summary>
        /// System.RevisedDate field.
        /// </summary>
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

        /// <summary>
        /// System.IterationId field.
        /// </summary>
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

        /// <summary>
        /// System.IterationPath field.
        /// </summary>
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

        /// <summary>
        /// System.WorkItemType field.
        /// </summary>
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

        /// <summary>
        /// System.State field.
        /// </summary>
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

        /// <summary>
        /// System.Reason field.
        /// </summary>
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

        /// <summary>
        /// System.CreatedDate field.
        /// </summary>
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

        /// <summary>
        /// System.CreatedBy field.
        /// </summary>
        public IdentityRef CreatedBy 
        {
            get 
            {
                this.Fields.TryGetValue("System.CreatedBy", out var val);
                return val as IdentityRef;
            }
        }

        /// <summary>
        /// System.ChangedDate field.
        /// </summary>
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

        /// <summary>
        /// System.ChangedBy field.
        /// </summary>
        public IdentityRef ChangedBy 
        {
            get 
            {
                this.Fields.TryGetValue("System.ChangedBy", out var val);
                return val as IdentityRef;
            }
        }

        /// <summary>
        /// Microsoft.VSTS.Common.Priority field.
        /// </summary>
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

        /// <summary>
        /// Microsoft.VSTS.Common.Severity field.
        /// </summary>
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

        /// <summary>
        /// Microsoft.VSTS.Scheduling.Effort field.
        /// </summary>
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

        /// <summary>
        /// System.Title field.
        /// </summary>
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

        /// <summary>
        /// System.Description field.
        /// </summary>
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
        
        /// <summary>
        /// System.Parent field.
        /// </summary>
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

        /// <summary>
        /// Microsoft.VSTS.Scheduling.RemainingWork field.
        /// </summary>
        public double? RemainingWork 
        {
            get 
            {
                this.Fields.TryGetValue("Microsoft.VSTS.Scheduling.RemainingWork", out var val);
                return val == null ? null : (double?)val;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("Microsoft.VSTS.Scheduling.RemainingWork");
                }
                else 
                {
                    this.Fields["Microsoft.VSTS.Scheduling.RemainingWork"] = value.Value;
                }
            }
        }

        /// <summary>
        /// Microsoft.VSTS.Scheduling.OriginalEstimate field.
        /// </summary>
        public double? OriginalEstimate 
        {
            get 
            {
                this.Fields.TryGetValue("Microsoft.VSTS.Scheduling.OriginalEstimate", out var val);
                return val == null ? null : (double?)val;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("Microsoft.VSTS.Scheduling.OriginalEstimate");
                }
                else 
                {
                    this.Fields["Microsoft.VSTS.Scheduling.OriginalEstimate"] = value.Value;
                }
            }
        }

        /// <summary>
        /// Microsoft.VSTS.Scheduling.CompletedWork field.
        /// </summary>
        public double? CompletedWork 
        {
            get 
            {
                this.Fields.TryGetValue("Microsoft.VSTS.Scheduling.CompletedWork", out var val);
                return val == null ? null : (double?)val;
            }
            set
            {
                if (value == null)
                {
                    this.Fields.Remove("Microsoft.VSTS.Scheduling.CompletedWork");
                }
                else 
                {
                    this.Fields["Microsoft.VSTS.Scheduling.CompletedWork"] = value.Value;
                }
            }
        }

        #endregion

        /// <summary>
        /// Snapshot of this object, in JSON form. Used for change detection.
        /// </summary>
        public JObject OriginalJson { get; protected set; }

        internal static WorkItemProxy FromWorkItem(WorkItem item, Type specificType = null)
        {
            var jObject = JObject.FromObject(item);
            var proxy = (WorkItemProxy)jObject.ToObject(specificType ?? typeof(WorkItemProxy));

            // Preserving the original values, to be able to detect changes later
            proxy.OriginalJson = jObject;

            // More initializations
            if (proxy.Relations == null)
            {
                proxy.Relations = new List<WorkItemRelation>();
            }

            return proxy;
        }

        public virtual JsonPatchDocument GetJsonPatchDocument(out int? originalRev)
        {
            var doc = new JsonPatchDocument();

            var original = this.OriginalJson == null ? new WorkItem() : this.OriginalJson.ToObject<WorkItem>();
            originalRev = original.Rev;

            // Fields

            foreach(var kv in this.Fields)
            {               
                if (original.Fields.ContainsKey(kv.Key))
                {
                    if (kv.Value != null)
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

            // Relations

            var relationsToBeAdded = new List<WorkItemRelation>();
            int i = 0;
            if (original.Relations != null)
            {
                for (; i < original.Relations.Count; i++)
                {
                    var originalRel = original.Relations[i];
                    var newRel = (this.Relations != null && i < this.Relations.Count) ? this.Relations[i] : null;

                    if (this.AreEqual(originalRel, newRel))
                    {
                        continue;
                    }

                    doc.Add(new JsonPatchOperation()
                    {
                        Operation = Operation.Remove,
                        Path = $"/relations/{i}"
                    });

                    if (newRel != null)
                    {
                        relationsToBeAdded.Add(newRel);
                    }
                }
            }

            if (this.Relations != null)
            {
                for (; i < this.Relations.Count; i++)
                {
                    relationsToBeAdded.Add(this.Relations[i]);
                }
            }

            foreach(var newRel in relationsToBeAdded)
            {
                doc.Add(new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = $"/relations/-",
                    Value = newRel
                });
            }

            return doc;
        }

        private bool AreDifferent(object oldValue, object newValue)
        {
            if (oldValue == null && newValue == null)
            {
                return false;
            }

            var type = newValue == null ? oldValue.GetType() : newValue.GetType();

            // Right now only values and strings are supported (complex types are not)
            if (!(type.IsValueType || type == typeof(string)))
            {
                return false;
            }

            return oldValue == null ? true : !oldValue.Equals(newValue);
        }

        private bool AreEqual(WorkItemRelation first, WorkItemRelation second)
        {
            if (first == null || second == null)
            {
                return false;
            }

            return
                first.Url?.ToLower() == second.Url?.ToLower() &&
                first.Rel == second.Rel &&
                first.Title == second.Title &&
                JToken.DeepEquals(JObject.FromObject(first.Attributes), JObject.FromObject(second.Attributes));
        }
    }
}