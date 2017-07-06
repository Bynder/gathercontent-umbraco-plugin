using System.Collections.Generic;
using GatherContent.Connector.Entities.Entities;

namespace GatherContent.Connector.Managers.Models.ImportItems
{
    public class FiltersModel
    {
        public Project CurrentProject { get; set; }

        public List<Project> Projects { get; set; }

        public List<GCTemplate> Templates { get; set; }

        public List<GCStatus> Statuses { get; set; }

        public FiltersModel() { }

        public FiltersModel(Project project, List<Project> projects, List<GCTemplate> templates, List<GCStatus> statuses)
        {
            CurrentProject = project;
            Projects = projects;
            Templates = templates;
            Statuses = statuses;
        }

    }
}
