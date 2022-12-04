using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
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
        /// <inheritdoc />
        public void Configure(IWebJobsBuilder builder)
        {
            // Registering VssConnectionFactory
            builder.Services.AddSingleton(new VssConnectionFactory());

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
        public ExtensionConfigProvider(VssConnectionFactory connectionFactory, TriggerExecutorRegistry executorRegistry, INameResolver nameResolver)
        {
            this._connectionFactory = connectionFactory;
            this._executorRegistry = executorRegistry;
            this._nameResolver = nameResolver;
        }

        /// <inheritdoc />
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
                .BindToInput<VssConnection>(attr => this._connectionFactory.GetVssConnection(attr));

            context
                .AddBindingRule<WorkItemClientAttribute>()
                .BindToInput<WorkItemTrackingHttpClient>(attr => attr.CreateClient(this._connectionFactory));

            var workItemRule = context.AddBindingRule<WorkItemAttribute>();
            workItemRule.BindToCollector(attr => new WorkItemCollector(this._connectionFactory, attr));
            workItemRule.BindToValueProvider(
                (attr, type) => Task.FromResult(new WorkItemValueProvider(this._connectionFactory, attr) as IValueBinder)
            );

            context
                .AddBindingRule<WorkItemsAttribute>()
                .BindToValueProvider(
                    (attr, type) => Task.FromResult(new WorkItemsValueProvider(this._connectionFactory, attr) as IValueBinder)
                );

            var testCaseRule = context.AddBindingRule<TestCaseAttribute>();
            testCaseRule.BindToCollector(attr => new WorkItemCollector(this._connectionFactory, attr));
            testCaseRule.BindToValueProvider(
                (attr, type) => Task.FromResult(new WorkItemValueProvider(this._connectionFactory, attr) as IValueBinder)
            );

            var testPlanRule = context.AddBindingRule<TestPlanAttribute>();
            testPlanRule.BindToCollector(attr => new TestPlanCollector(this._connectionFactory, attr));
            testPlanRule.BindToValueProvider(
                (attr, type) => Task.FromResult(new TestPlanValueProvider(this._connectionFactory, attr) as IValueBinder)
            );

            var testSuiteRule = context.AddBindingRule<TestSuiteAttribute>();
            testSuiteRule.BindToCollector(attr => new TestSuiteCollector(this._connectionFactory, attr));
            testSuiteRule.BindToValueProvider(
                (attr, type) => Task.FromResult(new TestSuiteValueProvider(this._connectionFactory, attr) as IValueBinder)
            );

            context
                .AddBindingRule<ProjectAttribute>()
                .BindToValueProvider(
                    (attr, type) => Task.FromResult(new ProjectValueProvider(this._connectionFactory, attr) as IValueBinder)
                );

            context
                .AddBindingRule<BuildDefinitionAttribute>()
                .BindToValueProvider(
                    (attr, type) => Task.FromResult(new BuildDefinitionValueProvider(this._connectionFactory, attr) as IValueBinder)
                );

            context
                .AddBindingRule<BuildClientAttribute>()
                // TODO: use async BindToInput() version
                .BindToInput<BuildHttpClient>(attr => attr.CreateClient(this._connectionFactory));

            context
                .AddBindingRule<GitClientAttribute>()
                .BindToInput<GitHttpClient>(attr => attr.CreateClient(this._connectionFactory));

            context
                .AddBindingRule<ProjectClientAttribute>()
                .BindToInput<ProjectHttpClient>(attr => attr.CreateClient(this._connectionFactory));

            context
                .AddBindingRule<ReleaseClientAttribute>()
                .BindToInput<ReleaseHttpClient>(attr => attr.CreateClient(this._connectionFactory));

            context.AddBindingRule<ReleaseEnvironmentAttribute>()
                .BindToCollector(attr => new ReleaseEnvironmentCollector(this._connectionFactory, attr));

            context.AddBindingRule<ReleaseEnvironmentStatusAttribute>()
                .BindToCollector(attr => new ReleaseEnvironmentStatusCollector(this._connectionFactory, attr));

            context
                .AddBindingRule<TestPlanClientAttribute>()
                .BindToInput<TestPlanHttpClient>(attr => attr.CreateClient(this._connectionFactory));

            context
                .AddBindingRule<WorkClientAttribute>()
                .BindToInput<WorkHttpClient>(attr => attr.CreateClient(this._connectionFactory));
        }

        private readonly VssConnectionFactory _connectionFactory;
        private readonly TriggerExecutorRegistry _executorRegistry;
        private readonly INameResolver _nameResolver;
    }
}