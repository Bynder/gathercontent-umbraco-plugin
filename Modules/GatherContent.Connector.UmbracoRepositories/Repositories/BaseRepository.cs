using Umbraco.Core;
using Umbraco.Core.Services;

namespace GatherContent.Connector.UmbracoRepositories.Repositories
{
    public class BaseRepository
    {
        protected readonly DatabaseContext ContextDatabase;
        protected readonly ServiceContext ContextService;

        protected BaseRepository()
        {
            ContextDatabase = ApplicationContext.Current.DatabaseContext;
            ContextService = ApplicationContext.Current.Services;
        }
    }
}
