using System.Threading.Tasks;

namespace AzFunc4DevOps.AzureDevOps
{
    public interface IGenericWatcherEntity<TParams> where TParams : GenericWatcherEntityParams
    {
        Task Watch(TParams parameters);
        void Delete();
    }
}