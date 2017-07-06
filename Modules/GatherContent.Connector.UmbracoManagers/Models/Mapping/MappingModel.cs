
using System.Collections.Generic;

namespace GatherContent.Connector.Managers.Models.Mapping
{
    public class MappingModel
    {
        public MappingModel()
        {
            FieldMappings = new List<FieldMappingModel>();
        }
        public string MappingId { get; set; }
        public string MappingTitle { get; set; }
        public GcProjectModel GcProject { get; set; }
        public GcTemplateModel GcTemplate { get; set; }
        public CmsTemplateModel CmsTemplate { get; set; }
        public string DefaultLocationId { get; set; }
        public string DefaultLocationTitle { get; set; }
        public string LastMappedDateTime { get; set; }
        public string LastUpdatedDate { get; set; }

        public IList<FieldMappingModel> FieldMappings { get; set; }   
    }
}
