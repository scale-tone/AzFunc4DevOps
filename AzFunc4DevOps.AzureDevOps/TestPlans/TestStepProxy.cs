using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace AzFunc4DevOps.AzureDevOps
{
    /*
        To clarify the decision to replace this Test Step object model with our own.
        The object model provided by TestManagement.WebApi is not extensible.
        E.g. ITestStep implementation doesn't have a public ctor => instances of that class
        can only be created via helpers => it doesn't fit into a normal paradigm, which is:

            myTestCase.TestSteps.Add(new TestStep { Title = "My Test Step" });

        Also e.g. ITestBase.SaveActions() _always adds_ attachments to the generated doc,
        even if those attachments were not modified. So this also needed to be overriden.
    */

    /// <summary>
    /// Represents a Test Case Step
    /// </summary>
    public class TestStepProxy
    {
        public string Title { get; set; }
        public string ExpectedResult { get; set; }
        public string Description { get; set; }
        public TestStepType TestStepType { get; set; }
        public int Id { get; private set; }
        public int? SharedStepId { get; set; }

        public ICollection<TestAttachmentProxy> Attachments { get; private set; }

        public TestStepProxy()
        {
            this.Attachments = new List<TestAttachmentProxy>();
        }

        internal TestStepProxy(ITestAction action)
        {
            this.UnderlyingAction = action;

            var sharedStep = action as ISharedStep;
            if (sharedStep != null)
            {
                // It might appear to be a _shared_ state. In that case just setting this property and quitting
                this.SharedStepId = sharedStep.SharedStepId;
                return;
            }

            var step = (ITestStep)action;

            this.Title = step.Title;
            this.ExpectedResult = step.ExpectedResult;
            this.Description = step.Description;
            this.TestStepType = step.TestStepType;
            this.Id = step.Id;

            this.Attachments = step.Attachments.Select(att => new TestAttachmentProxy(att)).ToList();
        }

        internal readonly ITestAction UnderlyingAction;

        internal void SaveChanges(
            TestCaseProxy parent, 
            JsonPatchDocument doc, 
            ref bool wasModified, 
            out ITestAction newUnderlyingStep 
        )
        {
            newUnderlyingStep = null;

            ITestStep underlyingStep;

            if (this.UnderlyingAction == null)
            {
                wasModified = true;

                if (this.SharedStepId.HasValue)
                {
                    // Treating this as a shared step
                    newUnderlyingStep = parent._helper.CreateSharedStepReference();
                    ((ISharedStep)newUnderlyingStep).SharedStepId = this.SharedStepId.Value;

                    return;
                }

                underlyingStep = parent._helper.CreateTestStep();
                newUnderlyingStep = underlyingStep;

                underlyingStep.Title = this.Title;
                underlyingStep.ExpectedResult = this.ExpectedResult;
                underlyingStep.Description = this.Description;
                underlyingStep.TestStepType = this.TestStepType;
            }
            else
            {
                var underlyingSharedStep = this.UnderlyingAction as ISharedStep;
                if (underlyingSharedStep != null)
                {
                    // Treating this as a shared step
                    if (underlyingSharedStep.SharedStepId != this.SharedStepId)
                    {
                        underlyingSharedStep.SharedStepId = this.SharedStepId.Value;
                        wasModified = true;
                    }

                    return;
                }

                underlyingStep = (ITestStep)this.UnderlyingAction;

                // Should only update underlying values if the property was actually modified.
                // That's because e.g. underlyingStep.Title now returns some extra unwanted HTML tags around the value.
                if (underlyingStep.Title != this.Title)
                {
                    underlyingStep.Title = this.Title;
                    wasModified = true;
                }
                if (underlyingStep.ExpectedResult != this.ExpectedResult)
                {
                    underlyingStep.ExpectedResult = this.ExpectedResult;
                    wasModified = true;
                }
                if (underlyingStep.Description != this.Description)
                {
                    underlyingStep.Description = this.Description;
                    wasModified = true;
                }
                if (underlyingStep.TestStepType != this.TestStepType)
                {
                    underlyingStep.TestStepType = this.TestStepType;
                    wasModified = true;
                }
            }

            // Need to remove all existing attachments from underlying collection,
            // otherwise ITestBase.SaveActions() will generate 'add' operations for them (which will subsequently fail)
            foreach (var ourAttachment in this.Attachments.Where(att => att.UnderlyingAttachment != null))
            {
                underlyingStep.Attachments.Remove(ourAttachment.UnderlyingAttachment);
            }

            // At this moment the underlying collection contains attachments that were removed.
            // Need to generate 'remove' actions for them ourselves, because ITestBase.SaveActions() doesn't do that.
            foreach (var theirAttachment in underlyingStep.Attachments)
            {
                parent.RemoveRelationsByUrl(doc, theirAttachment.Url, ref wasModified);
            }

            underlyingStep.Attachments.Clear();

            // Adding newly created attachments to the underlying collection
            foreach (var newAttachment in this.Attachments.Where(att => att.UnderlyingAttachment == null))
            {
                var theirAttachment = underlyingStep.CreateAttachment(newAttachment.Url, newAttachment.Name);
                theirAttachment.Comment = newAttachment.Comment;
                underlyingStep.Attachments.Add(theirAttachment);

                wasModified = true;
            }
        }
    }

    /// <summary>
    /// Represents a Test Case Step File Attachment
    /// </summary>
    public class TestAttachmentProxy
    {
        public string Comment { get; private set; }
        public string Name { get; private set; }
        public string Url { get; private set; }

        internal readonly ITestAttachment UnderlyingAttachment;

        internal TestAttachmentProxy(ITestAttachment att)
        {
            this.Comment = att.Comment;
            this.Name = att.Name;
            this.Url = att.Url;
            this.UnderlyingAttachment = att;
        }

        public TestAttachmentProxy(AttachmentReference reference)
        {
            this.Url = reference.Url;
        }
    }
}