using System.Collections.Generic;
using GatherContent.Connector.Managers.Models.Mapping;

namespace GatherContent.Connector.Managers.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMappingManager : IManager
    {
        MappingModel GetSingleMappingModel(string gcTemplateId, string cmsMappingId);

        List<MappingModel> GetMappingModel();

        List<GcProjectModel> GetAllGcProjects();

        List<GcProjectModel> GetGcProjectsWithMappings();

        List<CmsTemplateModel> GetAvailableTemplates();

        List<GcTemplateModel> GetTemplatesByProjectId(string gcProjectId);

        List<GcTabModel> GetFieldsByTemplateId(string gcTemplateId);

        void UpdateMapping(MappingModel model);

        void CreateMapping(MappingModel model);

        void DeleteMapping(string scMappingId);
    }
}