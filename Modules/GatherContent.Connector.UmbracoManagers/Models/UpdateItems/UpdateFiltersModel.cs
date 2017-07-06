using System.Collections.Generic;
using GatherContent.Connector.Managers.Models.Mapping;

namespace GatherContent.Connector.Managers.Models.UpdateItems
{
    public class UpdateFiltersModel
    {
        public UpdateFiltersModel()
        {
            Projects = new List<GcProjectModel>();
            Templates = new List<GcTemplateModel>();
            Statuses = new List<GcStatusModel>();
        }

        public List<GcProjectModel> Projects { get; set; }

        public List<GcTemplateModel> Templates { get; set; }

        public List<GcStatusModel> Statuses { get; set; }

    }
}
