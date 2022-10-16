using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Listeners;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Intentionally does nothing
    /// </summary>
    public class GenericListener : IListener
    {
        public void Cancel()
        {
        }

        public void Dispose()
        {
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }
    }
}