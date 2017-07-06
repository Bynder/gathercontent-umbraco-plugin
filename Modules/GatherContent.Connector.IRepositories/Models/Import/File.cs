using System;

namespace GatherContent.Connector.IRepositories.Models.Import
{
    public class File
    {
        public string FieldId { get; set; }

        public string Url { get; set; }
        public string FileName { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
