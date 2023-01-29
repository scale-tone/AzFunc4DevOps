# AzFunc4DevOps

A set of [Azure DevOps](https://learn.microsoft.com/en-us/azure/devops/user-guide/what-is-azure-devops) Triggers and Bindings for [Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/). Leverages [Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/) platform to simplify integration, automation, import/export and data synchronization scenarios for [Azure DevOps](https://learn.microsoft.com/en-us/azure/devops/user-guide/what-is-azure-devops).

[<img alt="Nuget" src="https://img.shields.io/nuget/v/AzFunc4DevOps.AzureDevOps?label=current%20version">](https://www.nuget.org/profiles/AzFunc4DevOps) 

[<img alt="Nuget" src="https://img.shields.io/nuget/v/AzFunc4DevOps.AzureDevOps?label=nuget%20downloads">](https://www.nuget.org/profiles/AzFunc4DevOps)

[![.NET](https://github.com/scale-tone/AzFunc4DevOps/actions/workflows/dotnet.yml/badge.svg)](https://github.com/scale-tone/AzFunc4DevOps/actions/workflows/dotnet.yml)

## How to use

As a prerequisite, you will need [Azure Functions Core Tools installed on your devbox](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local#install-the-azure-functions-core-tools).

#### 1. Create a local folder, name it e.g. `AzFunc4DevOpsTest` and initialize an Azure Functions .NET project in it:
``` 
  func init --worker-runtime dotnet
```

#### 2. Install [AzFunc4DevOps.AzureDevOps](https://www.nuget.org/packages/AzFunc4DevOps.AzureDevOps) NuGet package:
```
  dotnet add package AzFunc4DevOps.AzureDevOps
```

#### 3. Open the folder in Visual Studio Code:
```
  code .
```
Alternatively open the project in Visual Studio or any other IDE of your choice.

#### 4. In `local.settings.json` file configure the following required settings:
```
{
    "IsEncrypted": false,
    "Values": {
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",

        "AzureWebJobsStorage": "my-azure-storage-connection-string",

        "AZFUNC4DEVOPS_AZURE_DEVOPS_ORG_URL": "https://dev.azure.com/my-company-name",
        "AZFUNC4DEVOPS_AZURE_DEVOPS_PAT": "my-azure-devops-personal-access-token"
    }
}
```
  
  `AzureWebJobsStorage` needs to be configured, because AzFunc4DevOps internally uses [Azure Durable Functions](https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview), which require a storage. It's OK to use [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) for local development.
  
  `AZFUNC4DEVOPS_AZURE_DEVOPS_ORG_URL` is your Azure DevOps organization's full URL. E.g. `https://dev.azure.com/my-company-name`.
  
  `AZFUNC4DEVOPS_AZURE_DEVOPS_PAT` is your Azure DevOps Personal Access Token. [Create one in Azure DevOps portal](https://learn.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate#create-a-pat). Alternatively use [KeeShepherd](https://marketplace.visualstudio.com/items?itemName=kee-shepherd.kee-shepherd-vscode) tool for creating and safely handling it. 
    
  NOTE: the PAT needs to be given all [relevant scopes](https://learn.microsoft.com/en-us/azure/devops/integrate/get-started/authentication/oauth?view=azure-devops#scopes). E.g. if your Function is going to read/write Work Items, then `vso.work_write` will be needed.
  
  As an alternative to `AZFUNC4DEVOPS_AZURE_DEVOPS_ORG_URL` and `AZFUNC4DEVOPS_AZURE_DEVOPS_PAT` settings you can specify `OrgUrl` and `PersonalAccessToken` properties in every trigger/binding attribute. Those properties (just like any other trigger/binding attribute property) also support `%MY-SETTING-NAME%` syntax. See [the example here](https://github.com/scale-tone/AzFunc4DevOps/blob/main/samples/CloneBugsIntoDifferentOrg.cs#L23).


#### 5. Start adding Functions that use AzFunc4DevOps triggers and bindings. 
  E.g. the following Function adds `[Critical]` title prefix to a bug, once its `Severity` field changes to `1 - Critical`:
```
public static class AddCriticalToBugTitle
{
    [FunctionName(nameof(AddCriticalToBugTitle))]
    [return: WorkItem(Project = "MyProjectName")]
    public static WorkItemProxy Run
    (
        [WorkItemChangedTrigger
        (
            Project = "MyProjectName",
            WiqlQueryWhereClause = "[System.WorkItemType] = 'Bug'",
            FieldName = "Microsoft.VSTS.Common.Severity",
            ToValue = "1 - Critical"
        )]
        WorkItemChange change
    )
    {
        var item = change.NewVersion;

        if (!item.Title.StartsWith("[Critical]"))
        {
            item.Title = "[Critical] " + item.Title;
        }

        return item;
    }
}
```

#### 6. Run your Function locally:
```
  func start
```

## Samples

You can find more sample Functions [in the samples folder](https://github.com/scale-tone/AzFunc4DevOps/tree/main/samples#azfunc4devopsazuredevops-samples).


## Reference

See the [documentation in our Wiki](https://github.com/scale-tone/AzFunc4DevOps/wiki).


## Contributing

Is very much welcomed.
