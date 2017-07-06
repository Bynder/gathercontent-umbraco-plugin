using GatherContent.Connector.GatherContentService.Interfaces;
using GatherContent.Connector.Managers.Interfaces;
using GatherContent.Connector.UmbracoManagers.IoC;

namespace GatherContent.Connector.Managers.Managers
{
    /// <summary>
    /// 
    /// </summary>
    public class TestConnectionManager : BaseManager
    {
        

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool TestConnection()
        {
            try
            {
                FactoryService.Services.AccountsService.GetAccounts();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
