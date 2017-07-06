using GatherContent.Connector.Entities;

namespace GatherContent.Connector.Managers.Interfaces
{
    public interface IAccountSettingManager : IManager
    {
        GCAccountSettings GetSettings();
        void SetSettings(GCAccountSettings accountSettings);
        bool TestSettings();
    }
}