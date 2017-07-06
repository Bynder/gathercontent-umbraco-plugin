using System;
using System.Collections.Generic;
using GatherContent.Connector.Entities;
using GatherContent.Connector.IRepositories.Interfaces;

namespace GatherContent.Connector.UmbracoRepositories.IoC
{
    public static class FactoryRepository
    {
        private static Dictionary<string, Type> _configurableTypes = new Dictionary<string, Type>();

        private static Dictionary<string, Type> ConfigurableTypes
        {
            get
            {
                if (_configurableTypes == null || _configurableTypes.Count == 0)
                {
                    var configReader = new ConfigReader();
                    _configurableTypes = configReader.GetTypesFromConfig();
                }

                return _configurableTypes;
            }
        }

        private static T CreateInstance<T>(string registeredItem)
        {
            var dicItem = ConfigurableTypes[registeredItem];
            return (T)Activator.CreateInstance(dicItem);
        }

        public static class Repositories
        {
            public static IAccountsRepository AccountRepository
            {
                get { return CreateInstance<IAccountsRepository>("AccountsRepository"); }
            }

            public static IItemsRepository ItemsRepository
            {
                get { return CreateInstance<IItemsRepository>("ItemsRepository"); }
            }

            public static IMappingRepository MappingRepository
            {
                get { return CreateInstance<IMappingRepository>("MappingRepository"); }
            }

            public static GatherContent.Connector.IRepositories.Interfaces.IMediaRepository<object> MediaRepository
            {
                get { return CreateInstance<IMediaRepository<object>>("SimpleMediaRepository"); }
            }
        }
  
        
    }

  
}
