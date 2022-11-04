using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Triggered when a new Work Item is created.
    /// Currently retries every 1 minute FOREVER. Make sure to handle your exceptions.
    /// Use <see cref="WiqlQueryWhereClause"/> property to restrict the kind of Work Items to be triggered for.
    /// </summary>
    [Binding]
    public class WorkItemCreatedTriggerAttribute : GenericTriggerAttribute
    {
        /// <summary>
        /// WIQL WHERE filter.
        /// E.g. <example>"[Work Item Type] = 'User Story'"</example>.
        /// When specified, the Function will only be triggered for items, that satisfy that filter.
        /// <seealso href="https://learn.microsoft.com/en-us/azure/devops/boards/queries/wiql-syntax?view=azure-devops#specify-filter-clauses-where"/>
        /// </summary>
        [AutoResolve]
        public string WiqlQueryWhereClause { get; set; }
    }
}