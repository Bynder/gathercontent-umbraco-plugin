using System.Collections.Generic;
using GatherContent.Connector.Managers.Models.Mapping;

namespace GatherContent.Connector.Managers.Models.ImportItems.New
{
    public class FiltersModel
    {
        public FiltersModel()
        {
            Projects = new List<GcProjectModel>();
            Templates = new List<GcTemplateModel>();
            Statuses = new List<GcStatusModel>();
        }
        public GcProjectModel CurrentProject { get; set; }

        public List<GcProjectModel> Projects { get; set; }

        public List<GcTemplateModel> Templates { get; set; }

        public List<GcStatusModel> Statuses { get; set; }
    }
}
