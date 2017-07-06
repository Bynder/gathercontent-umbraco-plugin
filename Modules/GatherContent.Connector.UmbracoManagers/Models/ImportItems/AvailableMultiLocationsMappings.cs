
namespace GatherContent.Connector.Managers.Models.ImportItems
{

    public class AvailableMappingByLocation
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ScTemplate { get; set; }
        public string OpenerId { get; set; }
        public bool IsShowing { get; set; }
        public bool IsImport { get; set; }
        public string DefaultLocation { get; set; }


        public string DefaultLocationTitle { get; set; }
    }
}
