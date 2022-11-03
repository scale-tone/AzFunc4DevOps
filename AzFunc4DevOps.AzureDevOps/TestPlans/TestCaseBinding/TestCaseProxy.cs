using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <summary>
    /// Represents the 'Test Case' work item.
    /// </summary>
    public class TestCaseProxy : WorkItemProxy
    {
        /// <summary>
        /// List of Steps in this Test Case
        /// </summary>
        public ICollection<TestStepProxy> TestSteps
        {
            get 
            {
                if (this._steps == null)
                {
                    this._helper = new TestBaseHelper().Create();
                    
                    this.Fields.TryGetValue("Microsoft.VSTS.TCM.Steps", out var stepsFieldValue);
                    string stepsXml = (string)stepsFieldValue;

                    if (!string.IsNullOrWhiteSpace(stepsXml))
                    {
                        var links = this.Relations?.Select(r => new TestAttachmentLink
                        {
                            Title = r.Title,
                            Rel = r.Rel,
                            Url = r.Url,
                            Attributes = r.Attributes
                        })
                        .ToList();

                        // Using the helper to parse XML
                        this._helper.LoadActions(stepsXml, links);
                    }

                    this._steps = this._helper.Actions.Select(s => new TestStepProxy(s)).ToList();
                }

                return this._steps;
            }
        }

        public TestCaseProxy()
        {
            this.WorkItemType = "Test Case";
        }

        public override JsonPatchDocument GetJsonPatchDocument()
        {
            var doc = base.GetJsonPatchDocument();

            if (this._steps == null || this._helper == null)
            {
                // This means that TestSteps field wasn't touched, so doing nothing
                return doc;
            }

            bool isDirty = false;

            // Dropping removed steps
            int i = 0;
            while (i < this._helper.Actions.Count)
            {
                if (this._steps.Any(s => s.UnderlyingAction == this._helper.Actions[i]))
                {
                    i++;
                }
                else
                {
                    var removedUnderlyingAction = this._helper.Actions[i];
                    var removedUnderlyingStep = removedUnderlyingAction as ITestStep;

                    if (removedUnderlyingStep != null)
                    {
                        // Also need to remove attachments, if any
                        foreach(var removedAttachment in removedUnderlyingStep.Attachments)
                        {
                            this.RemoveRelationsByUrl(doc, removedAttachment.Url, ref isDirty);
                        }
                    }

                    this._helper.Actions.Remove(removedUnderlyingAction);
                    isDirty = true;
                }
            }

            // Persisting changes to existing steps and adding newly created steps
            foreach (var step in this._steps)
            {
                step.SaveChanges(this, doc, ref isDirty, out var newUnderlyingStep);

                if (newUnderlyingStep != null)
                {
                    this._helper.Actions.Add(newUnderlyingStep);
                }
            }

            if (isDirty)
            {
                this._helper.SaveActions(doc);
            }

            return doc;
        }

        private List<TestStepProxy> _steps;
        internal ITestBase _helper;

        internal void RemoveRelationsByUrl(JsonPatchDocument doc, string url, ref bool wasModified)
        {
            for (var i = 0; i < this.Relations.Count; i++)
            {
                var rel = this.Relations[i];
                if (rel.Url == url)
                {
                    doc.Add(new JsonPatchOperation()
                    {
                        Operation = Operation.Remove,
                        Path = $"/relations/{i}"
                    });

                    wasModified = true;
                }
            }
        }
    }
}