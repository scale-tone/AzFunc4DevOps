using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <summary>
    /// Represents a change in a Work Item. Used as a Function's parameter type for <see cref="WorkItemChangedTriggerAttribute"/>.
    /// </summary>
    public class WorkItemChange
    {
        /// <summary>
        /// The previous state of a Work Item.
        /// </summary>
        public WorkItemProxy OldVersion { get; private set; }

        /// <summary>
        /// The new state of a Work Item.
        /// </summary>
        public WorkItemProxy NewVersion { get; private set; }

        public WorkItemChange(WorkItem oldVersion, WorkItem newVersion)
        {
            this.OldVersion = WorkItemProxy.FromWorkItem(oldVersion);
            this.NewVersion = WorkItemProxy.FromWorkItem(newVersion);
        }
    }
}