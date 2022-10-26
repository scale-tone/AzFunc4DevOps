using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Newtonsoft.Json.Linq;

namespace AzFunc4DevOps.AzureDevOps
{
    public class ReleaseProxy : Release
    {
        public JObject OriginalJson { get; private set; }

        internal static ReleaseProxy FromRelease(Release item)
        {
            var jObject = JObject.FromObject(item);
            var proxy = jObject.ToObject<ReleaseProxy>();

            // Preserving the original values, to be able to detect changes later
            proxy.OriginalJson = jObject;

            return proxy;
        }
    }
}