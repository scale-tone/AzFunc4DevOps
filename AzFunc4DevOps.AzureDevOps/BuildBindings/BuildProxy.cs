using Microsoft.TeamFoundation.Build.WebApi;
using Newtonsoft.Json.Linq;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <summary>
    /// Represents the Build object.
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.teamfoundation.build.webapi.build"/>
    /// </summary>
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