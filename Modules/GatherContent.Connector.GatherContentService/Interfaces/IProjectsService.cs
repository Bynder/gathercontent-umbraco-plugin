using GatherContent.Connector.Entities.Entities;

namespace GatherContent.Connector.GatherContentService.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProjectsService : IService
    {
        ProjectsEntity GetProjects(int accountId);

        ProjectEntity GetSingleProject(string projectId);

        StatusesEntity GetAllStatuses(string projectId);

        StatusEntity GetSingleStatus(string statusId, string projectId);
    }
}
