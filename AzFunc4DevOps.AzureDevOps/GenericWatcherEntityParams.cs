using System;

namespace AzFunc4DevOps.AzureDevOps
{
    public class GenericWatcherEntityParams
    {
        public DateTimeOffset LastTimeExecuted { get; set; }
        public DateTimeOffset WhenToStop { get; set; }
    }
}