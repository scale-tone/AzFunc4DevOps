# AzFunc4DevOps Samples

A set of sample Azure Functions utilizing AzFunc4DevOps.AzureDevOps triggers and bindings.

* [AlignRemainingWorkWithCompletedWork](https://github.com/scale-tone/AzFunc4DevOps/blob/main/samples/AlignRemainingWorkWithCompletedWork.cs) - whenever Task's CompletedWork field changes, updates its RemainingWork field accordingly.
* [CreateTestCases](https://github.com/scale-tone/AzFunc4DevOps/blob/main/samples/CreateTestCases.cs) - creates Test Cases and assigns them to a given root Test Suite (identified by its testPlanId). HTTP-triggered, expects a JSON array of test cases in request's body.
* [MoveUnassignedBugsToCurrentSprint](https://github.com/scale-tone/AzFunc4DevOps/blob/main/samples/MoveUnassignedBugsToCurrentSprint.cs) - periodically collects all unassigned bugs and moves them into current iteration.
* [PassVariablesBetweenReleaseStages](https://github.com/scale-tone/AzFunc4DevOps/blob/main/samples/PassVariablesBetweenReleaseStages.cs) - demonstrates how to pass variables between Release Environments (Stages).
* [TriggerBuildWhenPullRequestCompleted](https://github.com/scale-tone/AzFunc4DevOps/blob/main/samples/TriggerBuildWhenPullRequestCompleted.cs) - triggers a Build once Pull Request is completed into master branch.
* [UpdateWorkItemDescription](https://github.com/scale-tone/AzFunc4DevOps/blob/main/samples/UpdateWorkItemDescription.cs) - updates Description field of a given workItem, identified by its workItemId. HTTP-triggered, expects the new Description value in request's body.

## How to run

* Clone this repo.
* Rename [local.settings.json.sample](https://github.com/scale-tone/AzFunc4DevOps/blob/main/samples/local.settings.json.sample) file to `local.settings.json` and adjust config settings in it accordingly.
* Run the Functions project in this folder: 
```
  func start
```

NOTE: this sample project references AzFunc4DevOps directly (as a CSPROJ-file). In your code you should instead install [AzFunc4DevOps.AzureDevOps](https://www.nuget.org/packages/AzFunc4DevOps.AzureDevOps) NuGet package.
