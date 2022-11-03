using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Triggers a Function when a Work Item is changed. Changes are detected by watching Work Item's revision numbers.
    /// Function's parameter should be of <see cref="WorkItemChange"/> type.
    /// Use <see cref="WiqlQueryWhereClause"/> property to restrict the kind of Work Items to be triggered for.
    /// </summary>
    [Binding]
    public class WorkItemChangedTriggerAttribute : GenericTriggerAttribute
    {
        /// <summary>
        /// WIQL WHERE filter.
        /// E.g. <example>"[Work Item Type] = 'User Story'"</example>.
        /// When specified, the Function will only be triggered for items, that satisfy that filter.
        /// <seealso href="https://learn.microsoft.com/en-us/azure/devops/boards/queries/wiql-syntax?view=azure-devops#specify-filter-clauses-where"/>
        /// </summary>
        [AutoResolve]
        public string WiqlQueryWhereClause { get; set; }

        /// <summary>
        /// Name of specific Field to watch for changes in.
        /// E.g. <example>"System.Title"</example>.
        /// When specified, the Function will only be triggered when that Field's value changes.
        /// </summary>
        [AutoResolve]
        public string FieldName { get; set; }

        /// <summary>
        /// Should be used together with <see cref="FieldName"/>.
        /// When specified, the Function will only be triggered when that Field's value changes FROM this value to some other value.
        /// </summary>
        [AutoResolve]
        public string FromValue { get; set; }

        /// <summary>
        /// Should be used together with <see cref="FieldName"/>.
        /// When specified, the Function will only be triggered when that Field's value changes TO this value.
        /// </summary>
        [AutoResolve]
        public string ToValue { get; set; }
    }
}