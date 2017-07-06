using System.Collections.Generic;
using GatherContent.Connector.IRepositories.Models.Import;

namespace GatherContent.Connector.IRepositories.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IItemsRepository : IRepository
    {
        IList<CmsItem> GetItems(string parentId, string language);

        CmsItem GetItem(string itemId, string language, bool readAllFields = false);

        string CreateMappedItem(string parentId, CmsItem cmsItem, string mappingId, string gcPath);

        string CreateNotMappedItem(string parentId, CmsItem cmsItem);

        void UpdateItem(CmsItem cmsItem);

        void MapText(CmsItem item, CmsField field);

        void MapChoice(CmsItem item, CmsField field);

        void MapFile(CmsItem item, CmsField field);

        bool IfMappedItemExists(string itemId, CmsItem cmsItem, string mappingId, string gcPath);

        bool IfMappedItemExists(string parentId, CmsItem cmsItem);

        bool IfNotMappedItemExists(string parentId, CmsItem cmsItem);

        string AddNewVersion(string itemId, CmsItem cmsItem, string mappingId, string gcPath);

        string GetCmsItemLink(string host, string itemId);

        string GetItemId(string parentId, CmsItem cmsItem);
    }
}
