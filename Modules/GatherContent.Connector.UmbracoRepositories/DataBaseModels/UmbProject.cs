using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace GatherContent.Connector.UmbracoRepositories.DataBaseModels
{
    [TableName("gcProject")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class UmbProject
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }
        public int GcProjectId { get; set; }
        public string GcProjectName { get; set; }
        [NullSetting(NullSetting = NullSettings.Null)]
        public string CreatedDate { get; set; }
        [NullSetting(NullSetting = NullSettings.Null)]
        public string UpdatedDate { get; set; }
        [NullSetting(NullSetting = NullSettings.Null)]
        public string User { get; set; }
    }
}
