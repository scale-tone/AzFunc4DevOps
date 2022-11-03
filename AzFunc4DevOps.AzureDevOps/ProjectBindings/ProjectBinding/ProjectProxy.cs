using Microsoft.TeamFoundation.Core.WebApi;
using Newtonsoft.Json.Linq;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <summary>
    /// Represents the TeamProject object.
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.teamfoundation.core.webapi.teamproject"/>
    /// </summary>
    public class ProjectProxy : TeamProject
    {
        internal static ProjectProxy FromTeamProject(TeamProject item)
        {
            var jObject = JObject.FromObject(item);
            var proxy = jObject.ToObject<ProjectProxy>();
            return proxy;
        }
    }
}