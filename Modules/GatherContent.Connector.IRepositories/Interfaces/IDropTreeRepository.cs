using System.Collections.Generic;
using GatherContent.Connector.IRepositories.Models.Import;

namespace GatherContent.Connector.IRepositories.Interfaces
{
    public interface IDropTreeRepository : IRepository
    {
        List<CmsItem> GetHomeNode(string id);
        List<CmsItem> GetChildren(string id);
    }
}
