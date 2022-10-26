using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
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
            builder.AddExtension(new ExtensionConfigProvider 
            { 
                _vssConnection = vssConnection,
                _executorRegistry = executorRegistry
            });
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
                    > (this._executorRegistry)
                );

            context
                .AddBindingRule<WorkItemChangedTriggerAttribute>()
                .BindToTrigger(
                    new GenericTriggerBindingProvider<
                        WorkItemChangedTriggerAttribute, 
                        GenericTriggerBinding<WorkItemChangedWatcherEntity, WorkItemChange>
                    > (this._executorRegistry)
                );

            context
                .AddBindingRule<BuildStatusChangedTriggerAttribute>()
                .BindToTrigger(
                    new GenericTriggerBindingProvider<
                        BuildStatusChangedTriggerAttribute, 
                        GenericTriggerBinding<BuildStatusChangedWatcherEntity, BuildProxy>
                    > (this._executorRegistry)
                );

            context
                .AddBindingRule<PullRequestStatusChangedTriggerAttribute>()
                .BindToTrigger(
                    new GenericTriggerBindingProvider<
                        PullRequestStatusChangedTriggerAttribute, 
                        GenericTriggerBinding<PullRequestStatusChangedWatcherEntity, PullRequestProxy>
                    > (this._executorRegistry)
                );

            context
                .AddBindingRule<ReleaseCreatedTriggerAttribute>()
                .BindToTrigger(
                    new GenericTriggerBindingProvider<
                        ReleaseCreatedTriggerAttribute, 
                        GenericTriggerBinding<ReleaseCreatedWatcherEntity, ReleaseProxy>
                    > (this._executorRegistry)
                );


            // Bindings

            context
                .AddBindingRule<WorkItemClientAttribute>()
                .BindToInput<WorkItemTrackingHttpClient>((_) => WorkItemClientAttribute.CreateClient(this._vssConnection));

            var workItemRule = context.AddBindingRule<WorkItemAttribute>();
            workItemRule.BindToCollector(attr => new WorkItemCollector(this._vssConnection, attr));
            workItemRule.BindToValueProvider(
                (attr, type) => Task.FromResult(new WorkItemValueProvider(this._vssConnection, attr) as IValueBinder)
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
        }

        /// <summary>
        /// Just to pass the VssConnection instance from Configure() to Initialize()
        /// </summary>
        private VssConnection _vssConnection;

        /// <summary>
        /// Just to pass the TriggerExecutorRegistry instance from Configure() to Initialize()
        /// </summary>
        private TriggerExecutorRegistry _executorRegistry;
    }
}