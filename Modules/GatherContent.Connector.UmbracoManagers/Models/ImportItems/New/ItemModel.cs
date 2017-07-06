using GatherContent.Connector.Managers.Models.Mapping;

namespace GatherContent.Connector.Managers.Models.ImportItems.New
{
   public class ItemModel
    {
       public ItemModel()
       {
           AvailableMappings = new AvailableMappings();
       }
        public GcItemModel GcItem { get; set; }
        public string Breadcrumb { get; set; }
        public GcTemplateModel GcTemplate { get; set; }
        public GcStatusModel Status { get; set; }
        public AvailableMappings AvailableMappings { get; set; }
    }
}
