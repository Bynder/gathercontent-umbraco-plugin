
namespace GatherContent.Connector.Managers.Models.ImportItems
{
    public class LocationImportItemModel
    {
        public string Id { get; set; }
        public string SelectedLocation { get; set; }
        public bool IsImport { get; set; }

        public string SelectedMappingId { get; set; }
    }
}
