using System.Collections.Generic;

namespace GatherContent.Connector.Managers.Models.Mapping
{
    public class GcTabModel
    {
        public GcTabModel()
        {
            Fields = new List<GcFieldModel>();
        }

        public string TabName { get; set; }
        public List<GcFieldModel> Fields { get; set; }
    }
}
