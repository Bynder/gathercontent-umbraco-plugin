using System;
using System.Collections.Generic;
using GatherContent.Connector.Entities;
using GatherContent.Connector.GatherContentService.Interfaces;
using GatherContent.Connector.GatherContentService.Services;
using GatherContent.Connector.IRepositories.Interfaces;
using GatherContent.Connector.Managers.Interfaces;
using GatherContent.Connector.Managers.Managers;
using GatherContent.Connector.UmbracoRepositories.IoC;

namespace GatherContent.Connector.UmbracoManagers.IoC
{
    public static class FactoryService
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

        public static GCAccountSettings CurrentSettings
        {
            get { return Managers.AccountSettingManager.GetSettings(); }
        }


        public static class Managers
        {

            public static IAccountSettingManager AccountSettingManager
            {
                get
                {
                    return new AccountSettingManager();
                }
            }

            public static ICacheManager CacheManager
            {
                get
                {
                    return new CacheManager();
                }
            }


            public static IImportManager ImportManager
            {
                get
                {
                    return new ImportManager();
                }
            }

            public static IMappingManager MappingManager
            {
                get
                {
                    return new MappingManager();
                }
            }


            public static IUpdateManager UpdateManager
            {
                get
                {
                    return new UpdateManager();
                }
            }
        }


        public static class Services
        {
            public static IAccountsService AccountsService
            {
                get
                {
                    return new AccountsService(CurrentSettings);
                }
            }

            public static IProjectsService ProjectsService
            {
                get
                {
                    return new ProjectsService(CurrentSettings);
                }
            }

            public static ITemplatesService TemplateService
            {
                get
                {
                    return new TemplatesService(CurrentSettings);
                }
            }

            public static IItemsService ItemsService
            {
                get
                {
                    return new ItemsService(CurrentSettings);
                }
            }
        }

        
    }


  
}
