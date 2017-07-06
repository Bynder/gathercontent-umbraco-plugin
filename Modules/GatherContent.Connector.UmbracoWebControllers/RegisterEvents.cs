using GatherContent.Connector.UmbracoRepositories.DataBaseModels;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace GatherContent.Connector.UmbracoWebControllers
{
    public class RegisterEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            var db = applicationContext.DatabaseContext.Database;

            //Check if the DB table does NOT exist
            if (!db.TableExist("gcAccountSettings"))
            {
                //Create DB table - and set overwrite to false
                db.CreateTable<UmbAccountSettings>(false);
            }
            if (!db.TableExist("gcProject"))
            {
                //Create DB table - and set overwrite to false
                db.CreateTable<UmbProject>(false);
            }
            if (!db.TableExist("gcTemplateMapping"))
            {
                db.CreateTable<UmbTemplateMapping>(false);
            }
            if (!db.TableExist("gcFieldMapping"))
            {
                db.CreateTable<UmbFieldMapping>(false);
            }

        }
    }
}