using GatherContent.Connector.Entities.Entities;

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
    }
}