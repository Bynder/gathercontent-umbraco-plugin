using System.Collections.Generic;
using GatherContent.Connector.IRepositories.Models.Mapping;

namespace GatherContent.Connector.IRepositories.Interfaces
{
    public interface IMappingRepository : IRepository
    {
        List<TemplateMapping> GetMappings();
        List<TemplateMapping> GetMappingsByGcProjectId(string projectId);
        List<TemplateMapping> GetMappingsByGcTemplateId(string gcTemplateId); 
        TemplateMapping GetMappingById(string id);


        List<CmsTemplate> GetAvailableCmsTemplates();


        void CreateMapping(TemplateMapping templateMapping);
        void UpdateMapping(TemplateMapping templateMapping);
        void DeleteMapping(string id);


    }
}
