using System;
using System.Threading.Tasks;

namespace AzFunc4DevOps.AzureDevOps
{
    public interface IGenericWatcherEntity
    {
        Task Watch(DateTimeOffset whenToStop);
    }
}