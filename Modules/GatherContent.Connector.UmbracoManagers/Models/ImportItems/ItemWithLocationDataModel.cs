using System.Collections.Generic;

namespace GatherContent.Connector.Managers.Models.ImportItems
{
    public class ItemWithLocationDataModel
    {
        public List<ImportItembyLocation> Items { get; set; }

        public ItemWithLocationDataModel() { }

        public ItemWithLocationDataModel(List<ImportItembyLocation> items)
        {
            Items = items;
        }
    }
}
