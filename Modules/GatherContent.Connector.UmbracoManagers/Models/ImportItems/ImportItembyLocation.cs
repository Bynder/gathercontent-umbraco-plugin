using System.Collections.Generic;
using System.Linq;
using GatherContent.Connector.Entities.Entities;

namespace GatherContent.Connector.Managers.Models.ImportItems
{
    public class ImportItembyLocation
    {
        public string Id { get; set; }

        public bool Checked { get; set; }

        public GCStatus Status { get; set; }

        public string Title { get; set; }

        public string LastUpdatedInGC { get; set; }

        public string Breadcrumb { get; set; }

        public GCTemplate Template { get; set; }

        public List<AvailableMappingByLocation> Mappings { get; set; }

        public ImportItembyLocation()
        {
            Mappings = new List<AvailableMappingByLocation>();
        }

        public ImportItembyLocation(GCItem item, GCTemplate template, List<GCItem> items, string dateFormat, IEnumerable<AvailableMappingByLocation> mappings)
        {
            Checked = false;

            Mappings = new List<AvailableMappingByLocation>();

            Id = item.Id.ToString();
            Title = item.Name;
            Status = item.Status.Data;
            Breadcrumb = GetBreadcrumb(item, items);
            LastUpdatedInGC = item.Updated.Date.ToString(dateFormat);
            Mappings.AddRange(mappings);
            Template = template;
        }

        private string GetBreadcrumb(GCItem item, List<GCItem> items)
        {
            var names = new List<string>();
            string result = BuildBreadcrumb(item, items, names);
            return result;
        }

        private string BuildBreadcrumb(GCItem item, List<GCItem> items, List<string> names)
        {
            names.Add(item.Name);

            if (item.ParentId != 0)
            {
                GCItem next = items.FirstOrDefault(i => i.Id == item.ParentId);
                return BuildBreadcrumb(next, items, names);
            }

            names.Reverse();

            string url = string.Join("/", names);

            return string.Format("/{0}", url);
        }

    }
}
