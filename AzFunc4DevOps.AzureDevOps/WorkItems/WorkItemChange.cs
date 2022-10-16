using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace AzFunc4DevOps.AzureDevOps
{
    public class WorkItemChange
    {
        public WorkItemProxy OldVersion { get; private set; }
        public WorkItemProxy NewVersion { get; private set; }

        public WorkItemChange(WorkItem oldVersion, WorkItem newVersion)
        {
            this.OldVersion = WorkItemProxy.FromWorkItem(oldVersion);
            this.NewVersion = WorkItemProxy.FromWorkItem(newVersion);
        }
    }
}