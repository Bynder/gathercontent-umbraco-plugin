using GatherContent.Connector.Entities;

namespace GatherContent.Connector.IRepositories.Interfaces
{
    public interface IAccountsRepository : IRepository
    {
        GCAccountSettings GetAccountSettings();
        void SetAccountSettings(GCAccountSettings accountSettings);
    }
}
