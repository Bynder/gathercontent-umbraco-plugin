using System;
using System.Net;
using System.Web.Http;
using GatherContent.Connector.Entities;
using GatherContent.Connector.Managers.Interfaces;
using GatherContent.Connector.Managers.Managers;
using GatherContent.Connector.UmbracoManagers.IoC;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;

namespace GatherContent.Connector.UmbracoWebControllers.Controllers
{
    [PluginController("GatherContent")]
    public class AccountSettingController : BaseController
    {

        [HttpGet]
        public IHttpActionResult Get()
        {
            try
            {
                return Ok(FactoryService.Managers.AccountSettingManager.GetSettings());
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
        public bool TestSettings()
        {
            try
            {
                return FactoryService.Managers.AccountSettingManager.TestSettings();
            }
            catch (Exception exception)
            {
                LogHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, exception.Message, exception);
            }
            return false;
        }

        [HttpPost]
        public IHttpActionResult Post(GCAccountSettings model)
        {
            if (model == null)
                BadRequest("Gather Content settings is null");
            try
            {
                FactoryService.Managers.AccountSettingManager.SetSettings(model);
                return Ok();
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
