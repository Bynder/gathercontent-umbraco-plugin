
using GatherContent.Connector.IRepositories.Models.Import;

namespace GatherContent.Connector.IRepositories.Models.Mapping
{
    public class FieldMapping
    {
        public CmsField CmsField { get; set; }
        public GcField GcField { get; set; }
    }
}
