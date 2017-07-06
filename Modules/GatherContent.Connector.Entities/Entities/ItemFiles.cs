using System.Collections.Generic;
using Newtonsoft.Json;

namespace GatherContent.Connector.Entities.Entities
{
    public class ItemFiles
    {
        [JsonProperty(PropertyName = "data")]
        public List<FileEntity> Data { get; set; }
    }
}
