using Newtonsoft.Json;

namespace GatherContent.Connector.Entities.Entities
{
    public class TemplateEntity
    {
        [JsonProperty(PropertyName = "data")]
        public GCTemplate Data { get; set; }
    }

}