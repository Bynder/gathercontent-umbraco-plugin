using GatherContent.Connector.Entities.Entities;

namespace GatherContent.Connector.Managers.Models.UpdateItems
{
    public class UpdateGCItem : GCItem
    {
        public string CMSId { get; set; }

        public UpdateGCItem(GCItem item, string cmsId)
        {
            this.CMSId = cmsId;
            this.Config = item.Config;
            this.Created = item.Created;
            this.CustomStateId = item.CustomStateId;
            this.ProjectId = item.ProjectId;
            this.DueDates = item.DueDates;
            this.Id = item.Id;
            this.Name = item.Name;
            this.Notes = item.Notes;
            this.Overdue = item.Overdue;
            this.Updated = item.Updated;
            this.Type = item.Type;
            this.ParentId = item.ParentId;
            this.Position = item.Position;
            this.Status = item.Status;
            this.TemplateId = item.TemplateId;
        }
    }
}
