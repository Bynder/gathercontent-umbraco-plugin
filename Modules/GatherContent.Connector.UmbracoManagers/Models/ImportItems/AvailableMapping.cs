
using System.Collections.Generic;
using GatherContent.Connector.Managers.Models.ImportItems.New;

namespace GatherContent.Connector.Managers.Models.ImportItems
{
    public class AvailableMappings
    {
        public AvailableMappings()
        {
            Mappings = new List<GatherContent.Connector.Managers.Models.ImportItems.New.AvailableMapping>();
        }
        public string SelectedMappingId { get; set; }
        public List<GatherContent.Connector.Managers.Models.ImportItems.New.AvailableMapping> Mappings { get; set; }
    } 
}
