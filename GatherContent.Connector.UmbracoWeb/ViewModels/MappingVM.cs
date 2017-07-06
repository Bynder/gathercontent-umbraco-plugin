using System;
using System.Collections.Generic;
using System.Net;
using System.Web;

namespace GatherContent.Connector.Umbraco.ViewModels
{
    public class MappingVM
    {
        public int Id { get; set; }
        public int GcProjectId { get; set; }
        public int MappingId { get; set; }
        public string GcProjectName { get; set; }
        public int GcTemplateId { get; set; }
        public string GcTemplateName { get; set; }
        public string CmsTemplateName { get; set; }
        public string LastMappedDateTime { get; set; }
        public string LastUpdatedDate { get; set; }
        public bool RemovedFromGc { get; set; }
        public bool IsMapped { get; set; }
        public bool IsHighlightingDate { get; set; }
    }
}