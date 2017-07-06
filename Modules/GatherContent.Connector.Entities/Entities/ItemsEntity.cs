using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GatherContent.Connector.Entities.Entities
{
    public class ItemsEntity
    {

        [JsonProperty(PropertyName = "data")]
        public List<GCItem> Data { get; set; }
    }

    public class GCItem
    {

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "project_id")]
        public int ProjectId { get; set; }

        [JsonProperty(PropertyName = "parent_id")]
        public int ParentId { get; set; }

        [JsonProperty(PropertyName = "template_id")]
        public int? TemplateId { get; set; }

        [JsonProperty(PropertyName = "custom_state_id")]
        public int CustomStateId { get; set; }

        [JsonProperty(PropertyName = "position")]
        public string Position { get; set; }

        [JsonProperty(PropertyName = "config")]
        public List<Config> Config { get; set; }

        [JsonProperty(PropertyName = "notes")]
        public string Notes { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "overdue")]
        public bool Overdue { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public ItemDate Created { get; set; }

        [JsonProperty(PropertyName = "updated_at")]
        public ItemDate Updated { get; set; }

        [JsonProperty(PropertyName = "status")]
        public StatusEntity Status { get; set; }

        [JsonProperty(PropertyName = "due_dates")]
        public DueDates DueDates { get; set; }

    }


    public class ItemDate
    {
        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "timezone_type")]
        public int TimezoneType { get; set; }

        [JsonProperty(PropertyName = "timezone")]
        public string Timezone { get; set; }
    }

    public class DueDates
    {

        //[JsonProperty(PropertyName = "data")]
        //public List<> Data { get; set; }

    }
}