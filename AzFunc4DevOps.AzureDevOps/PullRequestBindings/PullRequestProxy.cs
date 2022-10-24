using Microsoft.TeamFoundation.SourceControl.WebApi;
using Newtonsoft.Json.Linq;

namespace AzFunc4DevOps.AzureDevOps
{
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