using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GatherContent.Connector.Entities;
using GatherContent.Connector.GatherContentService.Interfaces;
using GatherContent.Connector.GatherContentService.Services;
using GatherContent.Connector.IRepositories.Interfaces;
using GatherContent.Connector.Managers.Interfaces;
using GatherContent.Connector.Managers.Managers;
using GatherContent.Connector.UmbracoManagers.IoC;
using GatherContent.Connector.UmbracoRepositories.IoC;
using GatherContent.Connector.UmbracoRepositories.Repositories;

namespace GatherContent.Connector.Managers.Managers
{
    public class AccountSettingManager : BaseManager, IAccountSettingManager
    {
        public GCAccountSettings GetSettings()
        {
            return FactoryRepository.Repositories.AccountRepository.GetAccountSettings();
        }

        public void SetSettings(GCAccountSettings accountSettings)
        {
            FactoryRepository.Repositories.AccountRepository.SetAccountSettings(accountSettings);
        }

        public bool TestSettings()
        {
            try
            {
                FactoryService.Services.AccountsService.GetAccounts();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
