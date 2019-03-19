using GatherContent.Connector.Entities.Entities;
using System.IO;

namespace GatherContent.Connector.GatherContentService.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IItemsService : IService
    {
        ItemsEntity GetItems(string projectId);

        ItemEntity GetSingleItem(string itemId);

        void ChooseStatusForItem(string itemId, string statusId);

        ItemFiles GetItemFiles(string itemId);

        Stream DownloadFile(int fileId);
    }
}