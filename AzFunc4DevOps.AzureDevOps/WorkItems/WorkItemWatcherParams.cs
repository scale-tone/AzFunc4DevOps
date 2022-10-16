using System;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AzFunc4DevOps.AzureDevOps
{
    public class WorkItemWatcherParams
    {
        public string ProjectName { get; set; }
        public string FieldName { get; set; }
        public string FromValue { get; set; }
        public string ToValue { get; set; }

        public int WorkItemId { get; set; }
        public EntityId ParentEntityId { get; set; }
        public DateTimeOffset WhenToStop { get; set; }
    }
}