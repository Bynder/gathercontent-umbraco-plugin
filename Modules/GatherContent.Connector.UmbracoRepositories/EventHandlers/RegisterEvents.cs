using GatherContent.Connector.UmbracoRepositories.DataBaseModels;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace GatherContent.Connector.UmbracoRepositories.EventHandlers
{
    public class RegisterEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //var db = applicationContext.DatabaseContext.Database;

            ////Check if the DB table does NOT exist
            //if (!db.TableExist("AccountSettings"))
            //{
            //    //Create DB table - and set overwrite to false
            //    db.CreateTable<UmbAccountSettings>(false);
            //}
            //if (!db.TableExist("Mapping"))
            //{
            //    //Create DB table - and set overwrite to false
            //    db.CreateTable<UmbMapping>(false);
            //}
            //if (!db.TableExist("Template"))
            //{
            //    db.CreateTable<UmbTemplate>(false);
            //}
            //if (!db.TableExist("FieldMapping"))
            //{
            //    db.CreateTable<UmbFieldMapping>(false);
            //}

        }
    }
}