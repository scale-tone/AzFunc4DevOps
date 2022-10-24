
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    public enum PullRequestStatusEnum
    {
        NotSet = 0,
        Active = 1,
        Abandoned = 2,
        Completed = 3,
        Draft = 4
    }
}