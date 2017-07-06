using System.Collections.Generic;
using GatherContent.Connector.Managers.Models.ImportItems.New;
using GatherContent.Connector.Managers.Models.UpdateItems;
using GatherContent.Connector.Managers.Models.UpdateItems.New;

namespace GatherContent.Connector.Managers.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUpdateManager : IManager
    {
        UpdateModel GetItemsForUpdate(string itemId, string languageId);

        List<ItemResultModel> UpdateItems(string itemId, List<UpdateListIds> models, string language);
    }
}