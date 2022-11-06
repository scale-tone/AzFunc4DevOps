using AzFunc4DevOps.AzureDevOps;
using Microsoft.Azure.WebJobs;
using Microsoft.TeamFoundation.Build.WebApi;
using System.Threading.Tasks;

namespace AzureDevOpsTestFunc
{
    public static class TriggerBuildWhenPullRequestCompleted
    {
        /*
            Triggers a Build once Pull Request is completed into master branch
        */
        [FunctionName(nameof(TriggerBuildWhenPullRequestCompleted))]
        public static async Task Run(
            // Whenever a Pull Request is completed into master branch
            [PullRequestStatusChangedTrigger
            (
                Project = "%TEAM_PROJECT_NAME%",
                TargetBranch = "master",
                ToValue = "Completed"
            )] 
            PullRequestProxy pullRequest,
            // Will need BuildHttpClient to trigger a build
            [BuildClient]
            BuildHttpClient buildClient,
            // Will need Project object to trigger a build
            [Project(Project = "%TEAM_PROJECT_NAME%")]
            ProjectProxy project,
            // Will be using this BuildDefinition
            [BuildDefinition(Project = "%TEAM_PROJECT_NAME%", Id = "%BUILD_DEFINITION_ID%")]
            BuildDefinitionProxy buildDefinition
        )
        {
            await buildClient.QueueBuildAsync(new Build
            {
                Definition = buildDefinition,
                Project = project
            });
        }
    }
}