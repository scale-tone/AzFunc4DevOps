using Microsoft.TeamFoundation.Core.WebApi;
using Newtonsoft.Json.Linq;

namespace AzFunc4DevOps.AzureDevOps
{
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