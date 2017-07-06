using System.Collections.Generic;
using Newtonsoft.Json;

namespace GatherContent.Connector.Entities.Entities
{
    public class ProjectsEntity
    {
        [JsonProperty(PropertyName = "data")]
        public List<Project> Data { get; set; }

    }

    public class Project
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "account_id")]
        public int AccountId { get; set; }

        [JsonProperty(PropertyName = "active")]
        public bool Active { get; set; }

        [JsonProperty(PropertyName = "text_direction")]
        public string TextDirection { get; set; }

        [JsonProperty(PropertyName = "allowed_tags")]
        public string AllowedTags { get; set; }

        [JsonProperty(PropertyName = "overdue")]
        public bool Overdue { get; set; }

        [JsonProperty(PropertyName = "statuses")]
        public StatusesEntity Statuses { get; set; }
    }

}