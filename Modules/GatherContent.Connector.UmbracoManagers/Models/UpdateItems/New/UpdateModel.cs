using System.Collections.Generic;

namespace GatherContent.Connector.Managers.Models.UpdateItems.New
{
    public class UpdateModel
    {
        public UpdateModel()
        {
            Items = new List<UpdateItemModel>();
        }
        public List<UpdateItemModel> Items { get; set; }
        public UpdateFiltersModel Filters { get; set; }

    }
}
