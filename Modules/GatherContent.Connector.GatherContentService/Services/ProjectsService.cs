using System.Net;
using GatherContent.Connector.Entities;
using GatherContent.Connector.Entities.Entities;
using GatherContent.Connector.GatherContentService.Interfaces;
using GatherContent.Connector.GatherContentService.Services.Abstract;

namespace GatherContent.Connector.GatherContentService.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class ProjectsService : BaseService, IProjectsService
    {
        protected override string ServiceUrl
        {
            get { return "projects"; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountSettings"></param>
        public ProjectsService(GCAccountSettings accountSettings)
            : base(accountSettings)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public ProjectsEntity GetProjects(int accountId)
        {
            string url = string.Format("{0}?account_id={1}", ServiceUrl, accountId);
            WebRequest webrequest = CreateRequest(url);
            webrequest.Method = WebRequestMethods.Http.Get;

            return ReadResponse<ProjectsEntity>(webrequest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public StatusesEntity GetAllStatuses(string projectId)
        {
            string url = string.Format("{0}/{1}/statuses", ServiceUrl, projectId);
            WebRequest webrequest = CreateRequest(url);
            webrequest.Method = WebRequestMethods.Http.Get;

            return ReadResponse<StatusesEntity>(webrequest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statusId"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public StatusEntity GetSingleStatus(string statusId, string projectId)
        {
            string url = string.Format("{0}/{1}/statuses/{2}", ServiceUrl, projectId, statusId);
            WebRequest webrequest = CreateRequest(url);
            webrequest.Method = WebRequestMethods.Http.Get;

            return ReadResponse<StatusEntity>(webrequest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public ProjectEntity GetSingleProject(string projectId)
        {
            string url = string.Format("{0}/{1}", ServiceUrl, projectId);
            WebRequest webrequest = CreateRequest(url);
            webrequest.Method = WebRequestMethods.Http.Get;

            return ReadResponse<ProjectEntity>(webrequest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="textDirection"></param>
        /// <param name="name"></param>
        public void PostProject(string accountId, string textDirection, string name)
        {
            var data = string.Format("account_id={0}&text_direction={1}&name={2}", accountId, textDirection, name);
            WebRequest webrequest = CreateRequest(ServiceUrl);
            webrequest.Method = WebRequestMethods.Http.Post;

            AddPostData(data, webrequest);

            ReadResponse(webrequest);
        }
    }
}
