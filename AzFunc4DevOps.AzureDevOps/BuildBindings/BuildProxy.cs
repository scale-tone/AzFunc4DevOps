using Microsoft.TeamFoundation.Build.WebApi;
using Newtonsoft.Json.Linq;

namespace AzFunc4DevOps.AzureDevOps
{
    public class BuildProxy : Build
    {
        internal static BuildProxy FromBuild(Build item)
        {
            var jObject = JObject.FromObject(item);
            var proxy = jObject.ToObject<BuildProxy>();
            return proxy;
        }
    }
}