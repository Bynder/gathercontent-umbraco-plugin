using System;
using System.IO;
using System.Net;
using System.Text;
using GatherContent.Connector.Entities;
using Newtonsoft.Json;

namespace GatherContent.Connector.GatherContentService.Services.Abstract
{
    public abstract class BaseService
    {
        protected virtual string ServiceUrl
        {
            get
            {
                return string.Empty;
            }
        }

        private static string _apiUrl;
        private static string _userName;
        private static string _apiKey;

        protected BaseService(GCAccountSettings accountSettings)
        {
            _apiUrl = accountSettings.ApiUrl;
            _apiKey = accountSettings.ApiKey;
            _userName = accountSettings.Username;
        }

        protected static WebRequest CreateRequest(string url)
        {
            if (!_apiUrl.EndsWith("/"))
            {
                _apiUrl = _apiUrl + "/";
            }
            HttpWebRequest webrequest = WebRequest.Create(_apiUrl + url) as HttpWebRequest;

            if (webrequest != null)
            {
                string token = GetBasicAuthToken(_userName, _apiKey);
                webrequest.Accept = "application/vnd.gathercontent.v0.5+json";
                webrequest.Headers.Add("Authorization", "Basic " + token);

                return webrequest;
            }

            return null;
        }

        private static string GetBasicAuthToken(string userName, string apiKey)
        {
            string tokenStr = string.Format("{0}:{1}", userName, apiKey);
            return Base64Encode(tokenStr);
        }

        private static string Base64Encode(string s)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            return Convert.ToBase64String(bytes);
        }

        protected static string ReadResponse(WebRequest webrequest)
        {
            using (Stream responseStream = webrequest.GetResponse().GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (var responseReader = new StreamReader(responseStream))
                    {
                        return responseReader.ReadToEnd();
                    }
                }
            }

            return null;
        }


        protected static T ReadResponse<T>(WebRequest webrequest) where T : class
        {
            T result = null;
            using (var responseStream = webrequest.GetResponse().GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (var responseReader = new StreamReader(responseStream))
                    {
                        var json = responseReader.ReadToEnd();
                        result = JsonConvert.DeserializeObject<T>(json);
                    }
                }
            }
            return result;
        }


        protected static void AddPostData(string data, WebRequest webrequest)
        {
            var byteArray = Encoding.UTF8.GetBytes(data);
            webrequest.ContentType = "application/x-www-form-urlencoded";
            webrequest.ContentLength = byteArray.Length;

            var dataStream = webrequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
        }
    }
}
