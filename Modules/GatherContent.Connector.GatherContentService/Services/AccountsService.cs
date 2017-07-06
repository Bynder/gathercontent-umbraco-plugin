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
    public class AccountsService : BaseService, IAccountsService
    {
        protected override string ServiceUrl
        {
            get { return "accounts"; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountSettings"></param>
        public AccountsService(GCAccountSettings accountSettings)
            : base(accountSettings)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public AccountEntity GetAccounts()
        {
            WebRequest webrequest = CreateRequest(ServiceUrl);
            webrequest.Method = WebRequestMethods.Http.Get;

            AccountEntity result = ReadResponse<AccountEntity>(webrequest);

            return result;
        }
    }
}
