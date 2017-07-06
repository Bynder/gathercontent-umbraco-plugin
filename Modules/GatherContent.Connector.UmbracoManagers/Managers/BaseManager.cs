using System.Collections.Generic;
using System.Linq;
using GatherContent.Connector.Entities.Entities;
using GatherContent.Connector.GatherContentService.Interfaces;
using GatherContent.Connector.GatherContentService.Services;
using GatherContent.Connector.Managers.Interfaces;
using GatherContent.Connector.UmbracoManagers.IoC;

namespace GatherContent.Connector.Managers.Managers
{
    /// <summary>
    /// 
    /// </summary>
    public class BaseManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected Account GetAccount()
        {
            var accounts = FactoryService.Services.AccountsService.GetAccounts();
            return accounts.Data.FirstOrDefault();
           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        protected List<Project> GetProjects(int accountId)
        {
            var projects = FactoryService.Services.ProjectsService.GetProjects(accountId);
            var activeProjects = projects.Data.Where(p => p.Active).ToList();
            return activeProjects;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected TemplateEntity GetGcTemplateEntity(string id)
        {
            TemplateEntity template;
            var key = "template_" + id;
            if (FactoryService.Managers.CacheManager.IsSet(key))
            {
                template = FactoryService.Managers.CacheManager.Get<TemplateEntity>(key);
            }
            else
            {
                template = FactoryService.Services.TemplateService.GetSingleTemplate(id);
                FactoryService.Managers.CacheManager.Set(key, template, 60);
            }
            return template;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected ProjectEntity GetGcProjectEntity(string id)
        {
            ProjectEntity project;
            var key = "project_" + id;
            if (FactoryService.Managers.CacheManager.IsSet(key))
            {
                project = FactoryService.Managers.CacheManager.Get<ProjectEntity>(key);
            }
            else
            {
                project = FactoryService.Services.ProjectsService.GetSingleProject(id);
                FactoryService.Managers.CacheManager.Set(key, project, 60);
            }
            return project;
        }
    }
}
