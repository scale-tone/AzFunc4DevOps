using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;

namespace AzFunc4DevOps.AzureDevOps
{
    public class GenericValueProvider<TValue> : IValueProvider
    {
        private readonly TValue _item;
        public GenericValueProvider(TValue item)
        {
            this._item = item;
        }

        public Type Type => typeof(TValue);

        public async Task<object> GetValueAsync()
        {
            return this._item;
        }

        public string ToInvokeString()
        {
            return this._item.ToString();
        }
    }
}