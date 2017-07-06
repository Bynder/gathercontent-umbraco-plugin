using System.Collections.Generic;
using Newtonsoft.Json;

namespace GatherContent.Connector.Entities.Entities
{
    public class StatusesEntity
    {
        [JsonProperty(PropertyName = "data")]
        public List<GCStatus> Data { get; set; }
    }
}