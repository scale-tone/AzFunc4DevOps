using Microsoft.TeamFoundation.SourceControl.WebApi;
using Newtonsoft.Json.Linq;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <summary>
    /// Represents the GitPullRequest object.
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.teamfoundation.sourcecontrol.webapi.gitpullrequest"/>
    /// </summary>
    public class PullRequestProxy : GitPullRequest
    {
        internal static PullRequestProxy FromPullRequest(GitPullRequest item)
        {
            var jObject = JObject.FromObject(item);
            var proxy = jObject.ToObject<PullRequestProxy>();
            return proxy;
        }
    }
}