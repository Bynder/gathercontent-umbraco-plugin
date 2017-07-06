using System;
using Newtonsoft.Json;

namespace GatherContent.Connector.Entities.Entities
{
    public class FileEntity
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "item_id")]
        public int ItemId { get; set; }

        [JsonProperty(PropertyName = "field")]
        public string Field { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "filename")]
        public string FileName { get; set; }

        [JsonProperty(PropertyName = "size")]
        public int Size { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public DateTime Created { get; set; }

        [JsonProperty(PropertyName = "updated_at")]
        public DateTime Updated { get; set; }

    }
}
