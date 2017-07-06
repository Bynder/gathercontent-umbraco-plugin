using System.Collections.Generic;
using GatherContent.Connector.IRepositories.Models.Import;

namespace GatherContent.Connector.IRepositories.Models.Mapping
{
    public class CmsTemplate
    {
        public CmsTemplate()
        {
            TemplateFields = new List<CmsTemplateField>();
        }
        public string TemplateId { get; set; }
        public string TemplateName { get; set; }
        public List<CmsTemplateField> TemplateFields { get; set; }
    }
}
