using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Newtonsoft.Json.Linq;

namespace AzFunc4DevOps.AzureDevOps
{
    public class ReleaseProxy : Release
    {
        internal static ReleaseProxy FromRelease(Release item)
        {
            var jObject = JObject.FromObject(item);
            var proxy = jObject.ToObject<ReleaseProxy>();
            return proxy;
        }
    }
}