using System.Collections.Generic;
using GatherContent.Connector.Entities.Entities;

namespace GatherContent.Connector.Managers.Models.ImportItems
{
    public class SelectItemsForImportWithLocation
    {
        public FiltersModel Filters { get; set; }

        public ItemWithLocationDataModel Data { get; set; }

        public SelectItemsForImportWithLocation(List<ImportItembyLocation> items, Project project, List<Project> projects, List<GCStatus> statuses, List<GCTemplate> templates)
        {
            Filters = new FiltersModel(project, projects, templates, statuses);
            Data = new ItemWithLocationDataModel(items);
        }
    }
}
