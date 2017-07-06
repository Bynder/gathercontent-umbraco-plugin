using System.Collections.Generic;
using GatherContent.Connector.IRepositories.Models.Mapping;

namespace GatherContent.Connector.IRepositories.Models.Import
{
   public class CmsItem
    {
       public CmsItem()
       {
           Children = new List<CmsItem>();
           Fields = new List<CmsField>();
       }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Language { get; set; }
        public CmsTemplate Template { get; set; }
        public IList<CmsField> Fields { get; set; }
        public List<CmsItem> Children { get; set; }
    }
}
