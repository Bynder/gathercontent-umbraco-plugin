using System.Collections.Generic;
using Newtonsoft.Json;

namespace GatherContent.Connector.Managers.Models.ImportItems
{
    public class DropTreeModel
    {
        public DropTreeModel()
        {
            Children = new List<DropTreeModel>();
        }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }
        [JsonProperty(PropertyName = "children")]
        public List<DropTreeModel> Children { get; set; }
        [JsonProperty(PropertyName = "isLazy")]
        public bool IsLazy { get; set; }
        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; set; }
        [JsonProperty(PropertyName = "select")]
        public bool Selected { get; set; }
        [JsonProperty(PropertyName = "expand")]
        public bool Expanded { get; set; }
    }
}
