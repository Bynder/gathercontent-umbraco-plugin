using Newtonsoft.Json;

namespace GatherContent.Connector.Entities.Entities
{
    public class ProjectEntity
    {
        [JsonProperty(PropertyName = "data")]
        public Project Data { get; set; }

        [JsonProperty(PropertyName = "meta")]
        public Meta Meta { get; set; }
    }

    public class Meta
    {
        [JsonProperty(PropertyName = "templates")]
        public int Templates { get; set; }
    }
}