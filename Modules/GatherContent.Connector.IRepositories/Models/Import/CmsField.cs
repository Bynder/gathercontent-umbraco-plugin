using System.Collections.Generic;

namespace GatherContent.Connector.IRepositories.Models.Import
{
    /// <summary>
    /// 
    /// </summary>
    public class CmsField
    {
        public CmsTemplateField TemplateField { get; set; }

        public object Value { get; set; }

        public List<string> Options { get; set; }

        public List<File> Files { get; set; }
    }
}
