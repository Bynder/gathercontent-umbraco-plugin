using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GatherContent.Connector.IRepositories.Interfaces;
using GatherContent.Connector.IRepositories.Models.Import;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using File = GatherContent.Connector.IRepositories.Models.Import.File;

namespace GatherContent.Connector.UmbracoRepositories.Repositories
{
    public class SimpleMediaRepository : BaseRepository, IMediaRepository<object>
    {
        public object UploadFile(string targetPath, File fileInfo)
        {
            string uri = fileInfo.Url.StartsWith("http") ? fileInfo.Url : "https://gathercontent.s3.amazonaws.com/" + fileInfo.Url;

            string extension = string.Empty;
            if (fileInfo.FileName.Contains("."))
            {
                extension = fileInfo.FileName.Substring(fileInfo.FileName.LastIndexOf('.') + 1).ToLower();
            }

            var request = (HttpWebRequest)WebRequest.Create(uri);
            var resp = (HttpWebResponse)request.GetResponse();
            var stream = resp.GetResponseStream();

            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                stream?.CopyTo(memoryStream);

                if (memoryStream.Length > 0)
                {
                    int media = CreateMedia(targetPath, fileInfo, extension, memoryStream);
                    return media;
                }
            }
            return null;
        }

        public virtual string ResolveMediaPath(CmsItem item, object createdItem, CmsField field)
        {
            var path = string.IsNullOrEmpty(field.TemplateField.FieldName)
                    ? string.Format("/GatherContent/{0}/", item.Title)
                    : string.Format("/GatherContent/{0}/{1}/", item.Title, field.TemplateField.FieldName);
            return path;
        }

        protected virtual int CreateMedia(string rootPath, File mediaFile, string extension, Stream mediaStream)
        {
            if (mediaStream.Length > 0)
            {
                rootPath += "/" + mediaFile.FileName;
                string[] path = rootPath.Split('/').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                IMedia itemMedia = null;
                for (int i = 0; i < path.Length; i++)
                {
                    var newItemMedia =
                        ContextService.MediaService.GetChildren(itemMedia?.Id ?? -1).FirstOrDefault(x => x.Name == path[i]);
                    if (newItemMedia == null)
                    {
                        string mediaType = (i < path.Length)
                            ? Constants.Conventions.MediaTypes.Folder
                            : Constants.Conventions.MediaTypes.File;

                        if (i == path.Length - 1 && UmbracoConfig.For.UmbracoSettings().Content.ImageFileTypes.Contains(extension))
                            mediaType = Constants.Conventions.MediaTypes.Image;

                        itemMedia = itemMedia != null ? ContextService.MediaService.CreateMedia(path[i], itemMedia, mediaType)
                            : ContextService.MediaService.CreateMedia(path[i], -1, mediaType);
                        ContextService.MediaService.Save(itemMedia);
                    }
                    else
                    {
                        itemMedia = newItemMedia;
                    }
                }
                if (itemMedia != null)
                {
                    itemMedia.SetValue(Constants.Conventions.Media.File, mediaFile.FileName, mediaStream);
                    ContextService.MediaService.Save(itemMedia);
                    return itemMedia.Id;
                }
            }
            return 0;
        }
    }
}
