using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml;
using Umbraco.Core.Logging;

namespace GatherContent.Connector.UmbracoRepositories.IoC
{

    public class ConfigReader
    {
        public Dictionary<string, Type> GetTypesFromConfig(string configName = "App_Config\\repositories.config")
        {
            if (string.IsNullOrEmpty(configName))
                throw new ArgumentNullException("configName");

            Dictionary<string, Type> types = new Dictionary<string, Type>();

            try
            {
                string appdata = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, configName);

                if (!File.Exists(appdata))
                {
                    LogHelper.Info(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, "GatherContent config not found in " + configName);
                    return null;
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(appdata);

                XmlNodeList listOfTypes = doc.SelectNodes("/configuration/components/component");

                if (listOfTypes != null && listOfTypes.Count > 0)
                {
                    foreach (XmlNode singleType in listOfTypes)
                    {
                        if (singleType.Attributes != null)
                        {
                            var id = singleType.Attributes["id"];
                            var type = singleType.Attributes["type"];

                            if (id != null && type != null && !string.IsNullOrEmpty(id.Value) && !string.IsNullOrEmpty(type.Value))
                            {
                                types.Add(id.Value, Type.GetType(type.Value));
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                LogHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, "msg", exception);
            }

            return types;
        }
    }
}
