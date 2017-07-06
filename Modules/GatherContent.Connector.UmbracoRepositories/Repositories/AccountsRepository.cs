using System.Linq;
using GatherContent.Connector.Entities;
using GatherContent.Connector.IRepositories;
using GatherContent.Connector.IRepositories.Interfaces;
using GatherContent.Connector.UmbracoRepositories.DataBaseModels;
using Umbraco.Core.Persistence;

namespace GatherContent.Connector.UmbracoRepositories.Repositories
{
    public class AccountsRepository : BaseRepository, IAccountsRepository
    {
        public GCAccountSettings GetAccountSettings()
        {
            var query = new Sql().Select("*").From<UmbAccountSettings>();
            var accountSettingU = this.ContextDatabase.Database.Fetch<UmbAccountSettings>(query).FirstOrDefault();
            if (accountSettingU != null)
            {
                return new GCAccountSettings()
                {
                    //AccountItemId = accountSettingU.AccountItemId,
                    ApiKey = accountSettingU.ApiKey,
                    ApiUrl = Constants.ApiUrl,
                    DateFormat = accountSettingU.DateFormat,
                    ImportDateFormat = accountSettingU.ImportDateFormat,
                    GatherContentUrl = accountSettingU.GatherContentUrl,
                    Username = accountSettingU.ApiUserName
                };
            }
            return new GCAccountSettings();
        }

        public void SetAccountSettings(GCAccountSettings accountSettings)
        {
            var settings = new UmbAccountSettings()
            {
                //AccountItemId = accountSettings.AccountItemId,
                ApiKey = accountSettings.ApiKey,
                //ApiUrl = Constants.ApiUrl,
                ApiUserName = accountSettings.Username,
                DateFormat = accountSettings.DateFormat,
                ImportDateFormat = accountSettings.ImportDateFormat,
                GatherContentUrl = accountSettings.GatherContentUrl
            };

            var query = new Sql().Select("*").From<UmbAccountSettings>();
            var accountSettingU = this.ContextDatabase.Database.Fetch<UmbAccountSettings>(query).FirstOrDefault();
            if (accountSettingU != null)
            {
                settings.Id = accountSettingU.Id;
                ContextDatabase.Database.Update("gcAccountSettings", "Id", settings);
            }
            else
            {
                ContextDatabase.Database.Save("gcAccountSettings", "Id", settings);
            }
        }
    }
}
