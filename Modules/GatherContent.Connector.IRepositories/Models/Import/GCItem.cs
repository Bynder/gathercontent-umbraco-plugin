using System.Collections.Generic;
using GatherContent.Connector.IRepositories.Models.Mapping;

namespace GatherContent.Connector.IRepositories.Models.Import
{
    public class GcItem
    {
        public string Id { get; set; }
        public GcTemplate Template { get; set; }

        public IList<GcField> Fields { get; set; }
    }
}
