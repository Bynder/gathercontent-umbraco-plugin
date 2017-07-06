using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace GatherContent.Connector.UmbracoRepositories.DataBaseModels
{
    [TableName("gcAccountSettings")]
    public class UmbAccountSettings
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }
        //public string ApiUrl { get; set; }
        public string ApiUserName { get; set; }
        public string ApiKey { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string DateFormat { get; set; }
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ImportDateFormat { get; set; }
        //public string AccountItemId { get; set; }
        public string GatherContentUrl { get; set; }
    }
}
