using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace GatherContent.Connector.UmbracoRepositories.DataBaseModels
{
    [TableName("gcTemplateMapping")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class UmbTemplateMapping
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }
        public int GcTemplateId { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string GcTemplateName { get; set; }

        public int CmsTemplateId { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string CmsTemplateName { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string MappingTitle { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string LastMappedDateTime { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string LastUpdatedDate { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string DefaultLocation { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string CreatedDate { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string UpdatedDate { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string User { get; set; }

        [ForeignKey(typeof(UmbProject), Name = "FK_gcTemplateMapping_gcProject")]
        public int ProjectId { get; set; }
    }
}
