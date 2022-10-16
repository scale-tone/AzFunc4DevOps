using System.Threading.Tasks;

namespace AzFunc4DevOps.AzureDevOps
{
    public interface IWorkItemWatcherEntity
    {
        Task Watch(WorkItemWatcherParams watcherParams);
        void Delete();
    }
}