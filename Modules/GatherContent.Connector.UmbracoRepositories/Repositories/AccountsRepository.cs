using System.Linq;
using System.Reflection;
using GatherContent.Connector.Entities;
using GatherContent.Connector.IRepositories;
using GatherContent.Connector.IRepositories.Interfaces;
using GatherContent.Connector.UmbracoRepositories.DataBaseModels;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence;

namespace GatherContent.Connector.UmbracoRepositories.Repositories
{
    public class AccountsRepository : BaseRepository, IAccountsRepository
    {
        public GCAccountSettings GetAccountSettings()
        {
            var query = new Sql().Select("*").From<UmbAccountSettings>();
            var accountSettingU = this.ContextDatabase.Database.Fetch<UmbAccountSettings>(query).FirstOrDefault();
			var gcAccountSettings = new GCAccountSettings();

			if (accountSettingU != null)
			{
                var tenantName = string.Empty;
                var tenantMatch = System.Text.RegularExpressions.Regex.Match(accountSettingU.GatherContentUrl, @"^(http(s)?:\/\/)?(?<tenant>.*)\.gathercontent\.com(\/)?$");
                if (tenantMatch.Groups["tenant"] != null)
                {
                    tenantName = tenantMatch.Groups["tenant"].Value.ToLower();
                }

                gcAccountSettings.ApiKey = accountSettingU.ApiKey;
				gcAccountSettings.ApiUrl = Constants.ApiUrl;
				gcAccountSettings.DateFormat = accountSettingU.DateFormat;
				gcAccountSettings.ImportDateFormat = accountSettingU.ImportDateFormat;
				gcAccountSettings.GatherContentUrl = accountSettingU.GatherContentUrl;
				gcAccountSettings.Username = accountSettingU.ApiUserName;
                gcAccountSettings.TenantName = tenantName;
            }

	        gcAccountSettings.CmsVersion = UmbracoVersion.Current.ToString();

			return gcAccountSettings;
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
