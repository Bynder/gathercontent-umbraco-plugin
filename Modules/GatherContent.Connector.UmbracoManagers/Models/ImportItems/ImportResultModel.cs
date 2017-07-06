using System.Collections.Generic;

namespace GatherContent.Connector.Managers.Models.ImportItems
{
    public class ImportResultModel
    {
        public List<MappingResultModel> Items { get; set; }

        public ImportResultModel(List<MappingResultModel> items)
        {
            Items = items;
        }
    }
}
