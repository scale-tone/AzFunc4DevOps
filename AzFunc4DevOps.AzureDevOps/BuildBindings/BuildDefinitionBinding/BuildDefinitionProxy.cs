using Microsoft.TeamFoundation.Build.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <summary>
    /// Represents a BuildDefinition instance.
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.teamfoundation.build.webapi.builddefinition"/>
    /// </summary>
    [JsonConverter(typeof(DummyJsonConverter))]
    public class BuildDefinitionProxy : BuildDefinition
    {
        internal static BuildDefinitionProxy FromDefinition(BuildDefinition item)
        {
            var jObject = JObject.FromObject(item);
            var proxy = jObject.ToObject<BuildDefinitionProxy>();
            return (BuildDefinitionProxy)proxy;
        }
    }

    /// <summary>
    /// Need to override base class'es JsonConverter on BuildDefinitionProxy with this one, otherwise it doesn't deserialize properly.
    /// </summary>
    internal class DummyJsonConverter : JsonConverter
    {
        public override bool CanRead => false;
        public override bool CanWrite => false;

        public override bool CanConvert(System.Type objectType)
        {
            return false;
        }

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new System.NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new System.NotImplementedException();
        }
    }
}