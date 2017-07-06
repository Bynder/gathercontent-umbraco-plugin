using GatherContent.Connector.Entities.Entities;

namespace GatherContent.Connector.GatherContentService.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITemplatesService : IService
    {
        TemplatesEntity GetTemplates(string projectId);

        TemplateEntity GetSingleTemplate(string templateId);
    }
}