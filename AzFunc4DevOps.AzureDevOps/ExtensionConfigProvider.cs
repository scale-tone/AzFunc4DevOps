using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

[assembly: WebJobsStartup(typeof(AzFunc4DevOps.AzureDevOps.ExtensionConfigProvider))]
namespace AzFunc4DevOps.AzureDevOps
{
    /// <summary>
    /// Acts both as IExtensionConfigProvider (provides custom trigger/binding info)
    /// and IWebJobsStartup (registers trigger/binding info at startup).
    /// First instance of this class is created by the runtime.
    /// Then that instance creates another instance, which then does IExtensionConfigProvider stuff.
    /// </summary>
    public class ExtensionConfigProvider : IExtensionConfigProvider, IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            // According to its source code, VssConnection should be reused.
            // Here we create a singleton instance of it, inject it into DI container
            // and also pass to our own Initialize() method, so that it can be used by bindings
            // (because bindings do not seem to have a way to access DI container).

            var vssConnection = new VssConnection(new Uri(Settings.AZURE_DEVOPS_ORG_URL), new VssBasicCredential(string.Empty, Settings.AZURE_DEVOPS_PAT));

            builder.Services.AddSingleton(vssConnection);

            // Also creating, injecting into DI and passing to Initialize() the instance of TriggerExecutorRegistry class
            // (it acts as a map between binding executors and their relevant entities)

            var executorRegistry = new TriggerExecutorRegistry();

            builder.Services.AddSingleton(executorRegistry);

            // Adding bindings
            builder.AddExtension<ExtensionConfigProvider>();
        }

        /// <summary>
        /// Used by IWebJobsStartup
        /// </summary>
        public ExtensionConfigProvider()
        {
        }

        /// <summary>
        /// Used by IExtensionConfigProvider
        /// </summary>
        public ExtensionConfigProvider(VssConnection vssConnection, TriggerExecutorRegistry executorRegistry, INameResolver nameResolver)
        {
            this._vssConnection = vssConnection;
            this._executorRegistry = executorRegistry;
            this._nameResolver = nameResolver;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            // Here is where attributes and bindings are welded together

            // Triggers

            context
                .AddBindingRule<WorkItemCreatedTriggerAttribute>()
                .BindToTrigger(
                    new GenericTriggerBindingProvider<
                        WorkItemCreatedTriggerAttribute, 
                        GenericTriggerBinding<WorkItemCreatedWatcherEntity, WorkItemProxy>
                    > (this._executorRegistry, this._nameResolver)
                );

            context
                .AddBindingRule<WorkItemChangedTriggerAttribute>()
                .BindToTrigger(
                    new GenericTriggerBindingProvider<
                        WorkItemChangedTriggerAttribute, 
                        GenericTriggerBinding<WorkItemChangedWatcherEntity, WorkItemChange>
                    > (this._executorRegistry, this._nameResolver)
                );

            context
                .AddBindingRule<BuildStatusChangedTriggerAttribute>()
                .BindToTrigger(
                    new GenericTriggerBindingProvider<
                        BuildStatusChangedTriggerAttribute, 
                        GenericTriggerBinding<BuildStatusChangedWatcherEntity, BuildProxy>
                    > (this._executorRegistry, this._nameResolver)
                );

            context
                .AddBindingRule<PullRequestStatusChangedTriggerAttribute>()
                .BindToTrigger(
                    new GenericTriggerBindingProvider<
                        PullRequestStatusChangedTriggerAttribute, 
                        GenericTriggerBinding<PullRequestStatusChangedWatcherEntity, PullRequestProxy>
                    > (this._executorRegistry, this._nameResolver)
                );

            context
                .AddBindingRule<ReleaseCreatedTriggerAttribute>()
                .BindToTrigger(
                    new GenericTriggerBindingProvider<
                        ReleaseCreatedTriggerAttribute, 
                        GenericTriggerBinding<ReleaseCreatedWatcherEntity, ReleaseProxy>
                    > (this._executorRegistry, this._nameResolver)
                );


            context
                .AddBindingRule<ReleaseEnvironmentStatusChangedTriggerAttribute>()
                .BindToTrigger(
                    new GenericTriggerBindingProvider<
                        ReleaseEnvironmentStatusChangedTriggerAttribute, 
                        GenericTriggerBinding<ReleaseEnvironmentStatusChangedWatcherEntity, ReleaseEnvironmentProxy>
                    > (this._executorRegistry, this._nameResolver)
                );

            // Bindings

            context
                .AddBindingRule<VssConnectionAttribute>()
                .BindToInput<VssConnection>((_) => this._vssConnection);

            context
                .AddBindingRule<WorkItemClientAttribute>()
                .BindToInput<WorkItemTrackingHttpClient>((_) => WorkItemClientAttribute.CreateClient(this._vssConnection));

            var workItemRule = context.AddBindingRule<WorkItemAttribute>();
            workItemRule.BindToCollector(attr => new WorkItemCollector(this._vssConnection, attr));
            workItemRule.BindToValueProvider(
                (attr, type) => Task.FromResult(new WorkItemValueProvider(this._vssConnection, attr) as IValueBinder)
            );

            context
                .AddBindingRule<WorkItemsAttribute>()
                .BindToValueProvider(
                    (attr, type) => Task.FromResult(new WorkItemsValueProvider(this._vssConnection, attr) as IValueBinder)
                );

            var testCaseRule = context.AddBindingRule<TestCaseAttribute>();
            testCaseRule.BindToCollector(attr => new WorkItemCollector(this._vssConnection, attr));
            testCaseRule.BindToValueProvider(
                (attr, type) => Task.FromResult(new WorkItemValueProvider(this._vssConnection, attr) as IValueBinder)
            );

            var testSuiteRule = context.AddBindingRule<TestSuiteAttribute>();
            testSuiteRule.BindToCollector(attr => new TestSuiteCollector(this._vssConnection, attr));
            testSuiteRule.BindToValueProvider(
                (attr, type) => Task.FromResult(new TestSuiteValueProvider(this._vssConnection, attr) as IValueBinder)
            );

            context
                .AddBindingRule<ProjectAttribute>()
                .BindToValueProvider(
                    (attr, type) => Task.FromResult(new ProjectValueProvider(this._vssConnection, attr) as IValueBinder)
                );

            context
                .AddBindingRule<BuildDefinitionAttribute>()
                .BindToValueProvider(
                    (attr, type) => Task.FromResult(new BuildDefinitionValueProvider(this._vssConnection, attr) as IValueBinder)
                );

            context
                .AddBindingRule<BuildClientAttribute>()
                // TODO: use async BindToInput() version
                .BindToInput<BuildHttpClient>((_) => BuildClientAttribute.CreateClient(this._vssConnection));

            context
                .AddBindingRule<GitClientAttribute>()
                .BindToInput<GitHttpClient>((_) => GitClientAttribute.CreateClient(this._vssConnection));

            context
                .AddBindingRule<ProjectClientAttribute>()
                .BindToInput<ProjectHttpClient>((_) => ProjectClientAttribute.CreateClient(this._vssConnection));

            context
                .AddBindingRule<ReleaseClientAttribute>()
                .BindToInput<ReleaseHttpClient>((_) => ReleaseClientAttribute.CreateClient(this._vssConnection));

            context.AddBindingRule<ReleaseEnvironmentAttribute>()
                .BindToCollector(attr => new ReleaseEnvironmentCollector(this._vssConnection, attr));

            context.AddBindingRule<ReleaseEnvironmentStatusAttribute>()
                .BindToCollector(attr => new ReleaseEnvironmentStatusCollector(this._vssConnection, attr));

            context
                .AddBindingRule<TestPlanClientAttribute>()
                .BindToInput<TestPlanHttpClient>((_) => TestPlanClientAttribute.CreateClient(this._vssConnection));

            context
                .AddBindingRule<WorkClientAttribute>()
                .BindToInput<WorkHttpClient>((_) => WorkClientAttribute.CreateClient(this._vssConnection));
        }

        private readonly VssConnection _vssConnection;
        private readonly TriggerExecutorRegistry _executorRegistry;
        private readonly INameResolver _nameResolver;
    }
}