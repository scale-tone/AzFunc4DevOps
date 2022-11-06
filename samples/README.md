# AzFunc4DevOps Samples

A set of sample Azure Functions utilizing AzFunc4DevOps.AzureDevOps triggers and bindings.

* [AlignRemainingWorkWithCompletedWork](https://github.com/scale-tone/AzFunc4DevOps/blob/main/samples/AlignRemainingWorkWithCompletedWork.cs) - whenever Task's CompletedWork field changes, updates its RemainingWork field accordingly.
* [CreateTestCases](https://github.com/scale-tone/AzFunc4DevOps/blob/main/samples/CreateTestCases.cs) - creates Test Cases and assigns them to a given root Test Suite (identified by its testPlanId). HTTP-triggered, expects a JSON array of test cases in request's body.
* [MoveUnassignedBugsToCurrentSprint](https://github.com/scale-tone/AzFunc4DevOps/blob/main/samples/MoveUnassignedBugsToCurrentSprint.cs) - periodically collects all unassigned bugs and moves them into current iteration.
* [PassVariablesBetweenReleaseStages](https://github.com/scale-tone/AzFunc4DevOps/blob/main/samples/PassVariablesBetweenReleaseStages.cs) - demonstrates how to pass variables between Release Environments (Stages).
* [TriggerBuildWhenPullRequestCompleted](https://github.com/scale-tone/AzFunc4DevOps/blob/main/samples/TriggerBuildWhenPullRequestCompleted.cs) - triggers a Build once Pull Request is completed into master branch.
* [UpdateWorkItemDescription](https://github.com/scale-tone/AzFunc4DevOps/blob/main/samples/UpdateWorkItemDescription.cs) - updates Description field of a given workItem, identified by its workItemId. HTTP-triggered, expects the new Description value in request's body.
