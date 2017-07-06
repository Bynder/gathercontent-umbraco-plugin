using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace GatherContent.Connector.UmbracoRepositories.DataBaseModels
{
    [TableName("gcFieldMapping")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class UmbFieldMapping
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string CmsTemplateFieldName { get; set; }
        public string CmsTemplateFieldId { get; set; }
        [NullSetting(NullSetting = NullSettings.Null)]
        public string CmsTemplateFieldType { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string GcFieldName { get; set; }
        public string GcFieldId { get; set; }
        [NullSetting(NullSetting = NullSettings.Null)]
        public string GcFieldType { get; set; }
        [NullSetting(NullSetting = NullSettings.Null)]
        public string CreatedDate { get; set; }
        [NullSetting(NullSetting = NullSettings.Null)]
        public string UpdatedDate { get; set; }
        [NullSetting(NullSetting = NullSettings.Null)]
        public string User { get; set; }

        [ForeignKey(typeof(UmbTemplateMapping), Name = "FK_gcFieldMapping_gcTemplateMapping")]
        public int TemplateMappingId { get; set; }
    }
}
