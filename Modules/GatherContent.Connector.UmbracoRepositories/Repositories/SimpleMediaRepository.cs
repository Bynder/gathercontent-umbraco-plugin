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
using GatherContent.Connector.GatherContentService.Services;

namespace GatherContent.Connector.UmbracoRepositories.Repositories
{
	public class SimpleMediaRepository : BaseRepository, IMediaRepository<object>
	{
		public object UploadFile(string targetPath, File fileInfo)
		{

            var gcsettings = new AccountsRepository().GetAccountSettings();
            var itemService = new ItemsService(gcsettings);

            string extension = string.Empty;
			if (fileInfo.FileName.Contains("."))
			{
				extension = fileInfo.FileName.Substring(fileInfo.FileName.LastIndexOf('.') + 1).ToLower();
			}
            
            var memoryStream = itemService.DownloadFile(fileInfo.FileId) as MemoryStream;
            try
            {
                if (memoryStream.Length > 0)
                {
                    var media = CreateMedia(targetPath, fileInfo, extension, memoryStream);
                    return media;
                }
            }
            finally
            {
                memoryStream.Close();
            }

			return null;
		}

		public virtual string ResolveMediaPath(CmsItem item, object createdItem, CmsField field)
		{
			var path = string.IsNullOrEmpty(field.TemplateField.FieldName)
					? $"/GatherContent/{item.Title}/"
				: $"/GatherContent/{item.Title}/{field.TemplateField.FieldName}/";
			return path;
		}

		protected virtual int CreateMedia(string rootPath, File mediaFile, string extension, Stream mediaStream)
		{
			if (mediaStream.Length > 0)
			{
				rootPath += "/" + mediaFile.FileName;
				string[] path = rootPath.Split('/').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
				IMedia parentFolder = null;

				IMedia itemMedia = null;
				for (int i = 0; i < path.Length; i++)
				{
					var newItemMedia =
						ContextService.MediaService.GetChildren(itemMedia?.Id ?? -1).FirstOrDefault(x => x.Name == path[i]);
					if (i == path.Length - 2)
					{
						parentFolder = newItemMedia;
					}
					if (newItemMedia == null)
					{
						string mediaType = (i < path.Length)
							? Constants.Conventions.MediaTypes.Folder
							: Constants.Conventions.MediaTypes.File;

						if (i == path.Length - 1 && IsImage(extension))
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
					try
					{
						itemMedia.SetValue(Constants.Conventions.Media.File, mediaFile.FileName, mediaStream);
					}
					catch (Exception)
					{
						ContextService.MediaService.Delete(itemMedia);
						var type = IsImage(extension) ? Constants.Conventions.MediaTypes.Image : Constants.Conventions.MediaTypes.File;
						itemMedia = ContextService.MediaService.CreateMedia(mediaFile.FileName, parentFolder?.Id ?? -1, type);
						itemMedia.SetValue(Constants.Conventions.Media.File, mediaFile.FileName, mediaStream);
					}
					ContextService.MediaService.Save(itemMedia);
					return itemMedia.Id;
				}
			}
			return 0;
		}

		private bool IsImage(string extension)
		{
			return UmbracoConfig.For.UmbracoSettings().Content.ImageFileTypes.Contains(extension);
		}
	}
}
