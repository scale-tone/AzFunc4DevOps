using AzFunc4DevOps.AzureDevOps;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using System.Linq;
using System.Threading.Tasks;

namespace AzFunc4DevOps.Samples
{
    public static class SetTestCaseExecutionResult
    {
        /*
            Marks a given Test Case as Passed or Failed.
            Sample cURL request:
                curl -X POST "http://localhost:7071/api/SetTestCaseExecutionResult?caseId=435&suiteId=582&planId=478&outcome=failed"            
        */
        [FunctionName(nameof(SetTestCaseExecutionResult))]
        public static async Task Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,

            [TestSuite(Project = "%TEAM_PROJECT_NAME%", Id = "{Query.suiteId}", PlanId = "{Query.planId}")]
            TestSuiteProxy testSuite,

            [TestCase(Project = "%TEAM_PROJECT_NAME%", Id = "{Query.caseId}")]
            TestCaseProxy testCase,

            [TestPlanClient]
            TestPlanHttpClient client
        )
        {
            bool isPassed = req.Query["outcome"] == "passed";

            // Need to get Test Points in the given Test Case. A Test Point is a Test Case with specific Configuration. 
            var testPoints = await client.GetPointsListAsync(testSuite.Project.Name, testSuite.PlanId, testSuite.Id, null, testCase.Id.ToString());

            // Preparing the request
            var testPointUpdateParams = testPoints.Select(p => new TestPointUpdateParams 
            {
                Id = p.Id,
                Results = new Results
                {
                    Outcome = isPassed ? Outcome.Passed : Outcome.Failed
                }
            });

            // Making the update request
            await client.UpdateTestPointsAsync(testPointUpdateParams, testSuite.Project.Name, testSuite.PlanId, testSuite.Id);
        }
    }
}
