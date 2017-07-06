using GatherContent.Connector.Managers.Models.ImportItems.New;
using GatherContent.Connector.Managers.Models.Mapping;

namespace GatherContent.Connector.Managers.Models.UpdateItems.New
{
   public class UpdateItemModel
    {
        public string CmsId { get; set; }
        public string Title { get; set; }
        public string LastUpdatedInCms { get; set; }
        public GcStatusModel Status { get; set; }
        public GcItemModel GcItem { get; set; }
        public GcProjectModel Project { get; set; }
        public GcTemplateModel GcTemplate { get; set; }
        public CmsTemplateModel CmsTemplate { get; set; }
        public string CmsLink { get; set; }
        public string GcLink { get; set; }


    }

}
