using System.Collections.Generic;
using GatherContent.Connector.Entities.Entities;
using GatherContent.Connector.IRepositories.Models.Import;

namespace GatherContent.Connector.Managers.Models.ImportItems
{
    public class ImportCMSField
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }

        public List<Option> Options { get; set; }

        public List<File> Files { get; set; }

        public ImportCMSField(string type, string name, string label, string value, List<Option> options, List<File> files)
        {
            Type = type;
            Name = name;
            Label = label;
            Value = value;
            Options = options;
            Files = files;
        }
    }
}
