using GatherContent.Connector.Entities.Entities;

namespace GatherContent.Connector.GatherContentService.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAccountsService : IService
    {
        AccountEntity GetAccounts();
    }
}