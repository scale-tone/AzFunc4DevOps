using System.Collections.Generic;
using AzFunc4DevOps.AzureDevOps;
using Microsoft.Azure.WebJobs;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;

namespace AzureDevOpsTestFunc
{
    /*
        Passes variables between Release Stages.

        To output a value from a Release Stage:
            1. Add a command line (bash, PowerShell, cmd) step with 'AzFunc4DevOps' in its name.
            2. In that step print '<AzFunc4DevOps.MyResult>my-value-123</AzFunc4DevOps.MyResult>' to stdout.
        
        AzFunc4DevOps will then collect that value from Stage's logs and it will appear here as succeededStage.ResultValues["MyResult"].
        Then the below code will pass it to the next stage as a variable named 'MyResult'.

        Sample bash step that outputs Result Values:

            steps:
            - bash: |
            echo '<AzFunc4DevOps.MyResult>my-value-123</AzFunc4DevOps.MyResult>'
            displayName: Send a result value to AzFunc4DevOps
    */
    public static class PassVariablesBetweenReleaseStages
    {
        [FunctionName(nameof(PassVariablesBetweenReleaseStages))]
        [return: ReleaseEnvironmentStatus(Project = "%TEAM_PROJECT_NAME%")]
        public static ReleaseEnvironmentStatus Run
        (
            [ReleaseEnvironmentStatusChangedTrigger
            (
                Project = "%TEAM_PROJECT_NAME%",
                ReleaseDefinitionId = "%RELEASE_DEFINITION_ID%",
                ToValue = "Succeeded"
            )] 
            ReleaseEnvironmentProxy succeededStage
        )
        {
            var nextStageNode = OrderedStageNames.Find(succeededStage.Name)?.Next;

            // If next stage not found, doing nothing
            if (nextStageNode == null)
            {
                return null;
            }

            // Picking next Release Stage by name
            var nextStage = succeededStage.AllEnvironments[nextStageNode.Value];

            // Passing ResultValues from previous stage to the next stage and triggering that next stage

            var nextStageStatus = new ReleaseEnvironmentStatus(nextStage.ReleaseId, nextStage.Id)
            {
                // To just pass variables (without triggering) comment out the below line. But in that case the next stage needs to be delayed a bit,
                // for variables to take effect. Use Deployment Gates to achieve that.
                Status = EnvironmentStatus.InProgress,

                Comment = "Triggered by AzFunc4DevOps"
            };

            foreach (var resultValue in succeededStage.ResultValues)
            {
                nextStageStatus.Variables[resultValue.Key] = new ConfigurationVariableValue
                {
                    Value = resultValue.Value
                };
            }

            return nextStageStatus;
        }

        /// <summary>
        /// Need to define the _order_ of Release Stages
        /// </summary>
        static readonly LinkedList<string> OrderedStageNames = new LinkedList<string>(new[] {
            "Stage 1",
            "Stage 2",
            "Stage 3"
        });
    }
}