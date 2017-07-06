using System.Collections.Generic;
using GatherContent.Connector.Managers.Models.ImportItems;

namespace GatherContent.Connector.Managers.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDropTreeManager : IManager
    {
        List<DropTreeModel> GetTopLevelNode(string id);

        List<DropTreeModel> GetChildrenNodes(string id);
    }
}