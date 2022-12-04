using AzFunc4DevOps.AzureDevOps;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Newtonsoft.Json;
using System.IO;

namespace AzFunc4DevOps.Samples
{
    public static class AddTestCasesToTestSuite
    {
        /*
            Creates Test Cases and assigns them to a given root Test Suite (identified by its testPlanId).
            Expects the following JSON in request body:

                [
                    {
                        "name": "MyTestCase1",
                        "steps": [
                            {
                                "action": "step one",
                                "result": "OK"
                            },
                            {
                                "action": "step two",
                                "result": "Fail"
                            }
                        ]
                    },
                    {
                        "name": "MyTestCase2",
                        "steps": [
                            {
                                "action": "step three",
                                "result": "Fail"
                            },
                            {
                                "action": "step four",
                                "result": "OK"
                            }
                        ]
                    }
                ]
        */
        [FunctionName(nameof(AddTestCasesToTestSuite))]
        // Need to return the given Test Suite, to indicate that changes to it need to be persisted
        [return: TestSuite(Project = "%TEAM_PROJECT_NAME%")]
        public static TestSuiteProxy Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,

            // Binding to a root Test Suite by testPlanId, which is expected to come as a query string parameter
            [TestSuite(Project = "%TEAM_PROJECT_NAME%", PlanId = "{Query.testPlanId}")]
            TestSuiteProxy testSuite,

            // Newly created Test Cases will be returned via this ICollector
            [TestCase(Project = "%TEAM_PROJECT_NAME%")]
            ICollector<TestCaseProxy> testCasesCollector
        )
        {
            using var reader = new StreamReader(req.Body);

            dynamic inputTestCases = JsonConvert.DeserializeObject(reader.ReadToEnd());

            foreach(var inputTestCase in inputTestCases)
            {
                var newTestCase = new TestCaseProxy
                {
                    Title = inputTestCase.name
                };

                foreach (var testStep in inputTestCase.steps)
                {
                    newTestCase.TestSteps.Add(new TestStepProxy { 
                        Title = testStep.action,
                        ExpectedResult = testStep.result
                    });
                }

                // Creating a new Test Case
                testCasesCollector.Add(newTestCase);

                // Assigning it to the root Test Suite of the given Test Plan
                testSuite.TestCases.Add(new TestCaseId(newTestCase.Id.Value));
            }

            // Returning the updated Test Suite, so that changes are persisted.
            return testSuite;
        }
    }
}
