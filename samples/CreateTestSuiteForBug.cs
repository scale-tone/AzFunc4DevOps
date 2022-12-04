using AzFunc4DevOps.AzureDevOps;
using Microsoft.Azure.WebJobs;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzFunc4DevOps.Samples
{
    public static class CreateTestSuiteForBug
    {
        /*
            Whenever a bug is created, creates a Test Suite for it.
        */
        [FunctionName(nameof(CreateTestSuiteForBug))]
        public static async Task Run (

            // When a bug is created
            [WorkItemCreatedTrigger
            (
                Project = "%TEAM_PROJECT_NAME%", 
                WiqlQueryWhereClause = "[System.WorkItemType] = 'Bug'"
            )] 
            WorkItemProxy bug,

            // The list of existing Test Plans, to check whether a Test Plan for given iteration already exists
            [WorkItems(
                Project = "%TEAM_PROJECT_NAME%", 
                WiqlQueryWhereClause = "[System.WorkItemType] = 'Test Plan'"
            )]
            IEnumerable<WorkItemProxy> existingTestPlanWorkItems,

            // For creating a new Test Plan
            [TestPlan(Project = "%TEAM_PROJECT_NAME%")]
            IAsyncCollector<TestPlanProxy> testPlanCollector,

            // For creating a new Test Suite
            [TestSuite(Project = "%TEAM_PROJECT_NAME%")]
            IAsyncCollector<TestSuiteProxy> testSuiteCollector
        )
        {
            int planId;

            // Checking if a Test Plan for this iteration already exists
            var existingTestPlanWorkItem = existingTestPlanWorkItems
                .FirstOrDefault(wi => wi.AreaPath == bug.AreaPath && wi.IterationPath == bug.IterationPath);

            if (existingTestPlanWorkItem != null)
            {
                planId = existingTestPlanWorkItem.Id.Value;
            }
            else
            {
                // Creating a new Test Plan for this iteration
                var newTestPlan = new TestPlanProxy
                {
                    Name = $"Test Plan for {bug.IterationPath}",
                    AreaPath = bug.AreaPath,
                    Iteration = bug.IterationPath
                };

                await testPlanCollector.AddAsync(newTestPlan);

                planId = newTestPlan.Id;
            }

            // Adding a Test Suite for this bug
            var testSuite = new TestSuiteProxy
            {
                PlanId = planId,
                SuiteType = TestSuiteType.RequirementTestSuite,
                RequirementId = bug.Id.Value
            };

            await testSuiteCollector.AddAsync(testSuite);
        }
    }
}
