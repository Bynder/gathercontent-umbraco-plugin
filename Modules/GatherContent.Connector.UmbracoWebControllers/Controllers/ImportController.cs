using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using GatherContent.Connector.Managers.Interfaces;
using GatherContent.Connector.Managers.Managers;
using GatherContent.Connector.Managers.Models.ImportItems;
using GatherContent.Connector.Managers.Models.Mapping;
using GatherContent.Connector.UmbracoManagers.IoC;
using Umbraco.Core.Logging;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace GatherContent.Connector.UmbracoWebControllers.Controllers
{
    [PluginController("GatherContent")]
    public class ImportController : BaseController
    {

        [System.Web.Http.HttpGet]
        public IHttpActionResult GetItems(string projectId = "", string templateId = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectId))
                    return Ok();

                return Ok(FactoryService.Managers.ImportManager.GetImportDialogModel(null, projectId));
            }
            catch (WebException exception)
            {
                LogHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, exception.Message, exception);
                return BadRequest(exception.Message + " Please check your credentials");
            }
            catch (Exception exception)
            {
                LogHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, exception.Message, exception);
                return BadRequest(exception.Message);
            }
        }

        [System.Web.Http.HttpGet]
        public IHttpActionResult GetFilters(int projectId)
        {
            try
            {
                return Ok(FactoryService.Managers.ImportManager.GetFilters(projectId.ToString()));
            }
            catch (WebException exception)
            {
                LogHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, exception.Message, exception);
                return BadRequest(exception.Message + " Please check your credentials");
            }
            catch (Exception exception)
            {
                LogHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, exception.Message, exception);
                return BadRequest(exception.Message);
            }
        }

        [HttpGet]
        public IHttpActionResult GetProjectsWithMapp()
        {
            try
            {
                return Ok(FactoryService.Managers.MappingManager.GetGcProjectsWithMappings());
            }
            catch (WebException exception)
            {
                LogHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, exception.Message, exception);
                return BadRequest(exception.Message + " Please check your credentials");
            }
            catch (Exception exception)
            {
                LogHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, exception.Message, exception);
                return BadRequest(exception.Message);
            }
        }

        [System.Web.Http.HttpPost]
        public IHttpActionResult Post([FromBody]List<ImportItemModel> items, [FromUri]string id, [FromUri]string projectId, [FromUri]string statusId)
        {
            try
            {
                return Ok(FactoryService.Managers.ImportManager.ImportItems(id, items, projectId, statusId, null));
            }
            catch (WebException exception)
            {
                LogHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, exception.Message, exception);
                return BadRequest(exception.Message + " Please check your credentials");
            }
            catch (Exception exception)
            {
                LogHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, exception.Message, exception);
                return BadRequest(exception.Message);
            }
        }

        [System.Web.Http.HttpPost]
        public IHttpActionResult PostWithLocations([FromBody]List<LocationImportItemModel> items, [FromUri]string projectId, [FromUri]string statusId)
        {
            try
            {
                return Ok(FactoryService.Managers.ImportManager.ImportItemsWithLocation(items, projectId, statusId, null));
            }
            catch (WebException exception)
            {
                LogHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, exception.Message, exception);
                return BadRequest(exception.Message + " Please check your credentials");
            }
            catch (Exception exception)
            {
                LogHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, exception.Message, exception);
                return BadRequest(exception.Message);
            }
        }
    }
}
