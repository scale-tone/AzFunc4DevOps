using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Newtonsoft.Json.Linq;

namespace AzFunc4DevOps.AzureDevOps
{
    public class ReleaseEnvironmentProxy : ReleaseEnvironment
    {
        public JObject OriginalJson { get; private set; }

        internal static ReleaseEnvironmentProxy FromReleaseEnvironment(ReleaseEnvironment item)
        {
            var jObject = JObject.FromObject(item);
            var proxy = jObject.ToObject<ReleaseEnvironmentProxy>();

            // Preserving the original values, to be able to detect changes later
            proxy.OriginalJson = jObject;

            return proxy;
        }
    }
}