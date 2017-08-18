using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using GatherContent.Connector.IRepositories.Interfaces;
using GatherContent.Connector.IRepositories.Models.Import;
using GatherContent.Connector.IRepositories.Models.Mapping;
using GatherContent.Connector.UmbracoRepositories.IoC;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using File = GatherContent.Connector.IRepositories.Models.Import.File;

namespace GatherContent.Connector.UmbracoRepositories.Repositories
{
    public class ItemsRepository : BaseRepository, IItemsRepository
    {
        public IList<CmsItem> GetItems(string parentId, string language)
        {
            if (string.IsNullOrWhiteSpace(parentId))
                return null;

            var result = new List<CmsItem>();
            int id = Convert.ToInt32(parentId);

            foreach (var content in ContextService.ContentService.GetChildren(id))
            {
                if (ContextService.ContentService.HasChildren(id))
                    result.AddRange(GetItems(content.Id.ToString(), null));
                if (content.HasProperty("gCContentId"))
                    result.Add(GetItem(content.Id.ToString(), null, false));
            }
            return result;
        }

        public CmsItem GetItem(string itemId, string language, bool readAllFields = false)
        {
            var content = ContextService.ContentService.GetById(Convert.ToInt32(itemId));
            var cmsTemplateFields = new List<CmsTemplateField>();
            cmsTemplateFields.AddRange(content.ContentType.CompositionPropertyTypes.Select( //get all composition properties of type
            type =>
                new CmsTemplateField()
                {
                    FieldId = type.Id.ToString(),
                    FieldName = type.Name,
                    FieldType =
                        ContextService.DataTypeService.GetDataTypeDefinitionById(
                            type.DataTypeDefinitionId).PropertyEditorAlias
                }));

            try
            {
                var item = new CmsItem()
                {
                    Id = content.Id.ToString(),
                    Title = content.Name,
                    Template = new CmsTemplate()
                    {
                        TemplateId = content.ContentType.Id.ToString(),
                        TemplateName = content.ContentType.Name,
                        TemplateFields = cmsTemplateFields
                    },
                    Fields = new List<CmsField>()
                    {
                        new CmsField
                        {
                            TemplateField = new CmsTemplateField {FieldName = "GC Content Id"},
                            Value = content.GetValue("gCContentId")
                        },
                        new CmsField
                        {
                            TemplateField = new CmsTemplateField {FieldName = "Last Sync Date"},
                            Value = content.UpdateDate
                        },
                        new CmsField
                        {
                            TemplateField = new CmsTemplateField {FieldName = "Template"},
                            Value = content.ContentType.Alias
                        }
                    }
                };
                return item;
            }
            catch (Exception ex)
            {
                throw new Exception("This template does not have 'GC Content Id' property.");
            }
        }

        public string CreateMappedItem(string parentId, CmsItem cmsItem, string mappingId, string gcPath)
        {
            if (!string.IsNullOrWhiteSpace(parentId))
            {
                var parentContent = ContextService.ContentService.GetById(Convert.ToInt32(parentId));
                if (parentContent != null)
                {
                    var contentType =
                        ContextService.ContentTypeService.GetContentType(Convert.ToInt32(cmsItem.Template.TemplateId));
                    contentType.AddPropertyGroup("GC Metadata");

                    contentType.AddPropertyType(
                        new PropertyType(
                            ContextService.DataTypeService.GetDataTypeDefinitionByPropertyEditorAlias("Umbraco.Textbox").FirstOrDefault())
                        {
                            Alias = "gCContentId",
                            Name = "GC Content Id",
                            Description = "",
                            Mandatory = false,
                            SortOrder = 1
                        }, "GC Metadata");

                    contentType.AddPropertyType(
                        new PropertyType(
                            ContextService.DataTypeService.GetDataTypeDefinitionByPropertyEditorAlias("Umbraco.Textbox").FirstOrDefault())
                        {
                            Alias = "mappingId",
                            Name = "Mapping Id",
                            Description = "",
                            Mandatory = false,
                            SortOrder = 2
                        }, "GC Metadata");

                    ContextService.ContentTypeService.Save(contentType);

                    var newContent = ContextService.ContentService.CreateContent(cmsItem.Title, parentContent,
                        contentType.Alias);

                    newContent.SetValue("gCContentId",
                        cmsItem.Fields.Single(x => x.TemplateField.FieldName == "GC Content Id").Value);
                    newContent.SetValue("mappingId", mappingId);

                    return ContextService.ContentService.SaveAndPublishWithStatus(newContent).Result.ContentItem.Id.ToString();
                }
            }
            return null;
        }

        public string CreateNotMappedItem(string parentId, CmsItem cmsItem)
        {
            if (string.IsNullOrWhiteSpace(parentId) && cmsItem.Fields.Single(x => x.TemplateField.FieldName == "Template").Value == null)
                return string.Empty;

            var parentContent = ContextService.ContentService.GetById(Convert.ToInt32(parentId));
            if (parentContent != null)
            {
                var contentType =
                    ContextService.ContentTypeService.GetContentType(Convert.ToInt32(cmsItem.Template.TemplateId));

                var newContent = ContextService.ContentService.CreateContent(cmsItem.Title, parentContent,
                    contentType.Alias);

                return ContextService.ContentService.SaveAndPublishWithStatus(newContent).Result.ContentItem.Id.ToString();
            }
            return null;
        }

        public void UpdateItem(CmsItem cmsItem)
        {
            if (cmsItem == null)
                return;


        }

        public void MapText(CmsItem item, CmsField cmsField)
        {
            IContent createdItem = ContextService.ContentService.GetById(Convert.ToInt32(item.Id));
            if (createdItem == null)
                return;

            var field =
                createdItem.PropertyTypes.FirstOrDefault(type => type.Id == Convert.ToInt32(cmsField.TemplateField.FieldId));

            if (field != null)
            {
                var value = field.PropertyEditorAlias == "Umbraco.TinyMCEv3"
                ? cmsField.Value.ToString().Trim()
                : Regex.Replace(cmsField.Value.ToString(), "<.*?>", string.Empty).Trim();

                switch (field.PropertyEditorAlias)
                {
                    case "Umbraco.Textbox":
                        if (value.Length > 500)
                            value = value.Substring(0, 500);
                        createdItem.SetValue(field.Alias, value);
                        break;

                    case "Umbraco.TextboxMultiple":
                        createdItem.SetValue(field.Alias, value);
                        break;

                    case "Umbraco.TinyMCEv3":
                        createdItem.SetValue(field.Alias, value);
                        break;

                    case "Umbraco.Date":
                        string dateString = Regex.Replace(cmsField.Value.ToString(), "<.*?>", string.Empty).Trim();
                        string expectedFormat = FactoryRepository.Repositories.AccountRepository.GetAccountSettings().ImportDateFormat;
                        try
                        {
                            dateString = dateString.Substring(0, 10);
                        }
                        catch (Exception e)
                        {
                            throw new ArgumentOutOfRangeException(String.Format("Error importing the date. Date must be in the format {0}", expectedFormat));
                        }
                        DateTime theDate;
                        bool result = DateTime.TryParseExact(dateString, expectedFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out theDate);
                        if (result)
                        {
                            createdItem.SetValue(field.Alias, theDate);
                        }
                        else
                        {
                            throw new ArgumentException(String.Format("Error importing the date. Date must be in the format {0}", expectedFormat));
                        }
                        break;
                    default:
                        break;
                }
                ContextService.ContentService.Save(createdItem);
            }
            else
            {
                throw new KeyNotFoundException(String.Format("Some fields has been deleted from Document Type {0}", createdItem.ContentType.Name));
            }
        }

        public void MapChoice(CmsItem item, CmsField cmsField)
        {
            IContent createdItem = ContextService.ContentService.GetById(Convert.ToInt32(item.Id));
            if (createdItem == null || cmsField.TemplateField == null || cmsField.Options == null || cmsField.Options.Count == 0)
                return;
            var field = createdItem.PropertyTypes.SingleOrDefault(
                type => type.Id == Convert.ToInt32(cmsField.TemplateField.FieldId));
            if (field != null)
            {
                IDataTypeDefinition dataType =
                    ContextService.DataTypeService.GetDataTypeDefinitionById(field.DataTypeDefinitionId);
                var dataSource =
                    ContextService.DataTypeService.GetPreValuesCollectionByDataTypeId(dataType.Id).PreValuesAsDictionary;

                switch (dataType.PropertyEditorAlias)
                {
                    case "Umbraco.RadioButtonList":
                    case "Umbraco.DropDown":
                    case "Umbraco.DropdownlistPublishingKeys":
                        var selected = dataSource.FirstOrDefault(x => x.Value.Value == cmsField.Options.First());
                        if (selected.Value != null)
                            createdItem.SetValue(
                                createdItem.PropertyTypes.Single(
                                    type => type.Id == Convert.ToInt32(cmsField.TemplateField.FieldId)).Alias,
                                selected.Value.Id);
                        break;
                    case "Umbraco.MultipleMediaPicker":
                    case "Umbraco.MediaPicker":
                        {
                            string imageIds = string.Empty;
                            if (cmsField.Files != null && cmsField.Files.Any())
                            {
                                foreach (var file in cmsField.Files)
                                {
                                    var fileName = file.FileName;
                                    var ext = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
                                    cmsField.TemplateField.FieldName = createdItem.PropertyTypes.Single(type =>
                                                    type.Id == Convert.ToInt32(cmsField.TemplateField.FieldId)).Name;
                                    var path = FactoryRepository.Repositories.MediaRepository.ResolveMediaPath(item, createdItem, cmsField);

                                    if (!UmbracoConfig.For.UmbracoSettings().Content.DisallowedUploadFiles.Contains(ext))
                                    {
                                        switch (dataType.PropertyEditorAlias)
                                        {
                                            case "Umbraco.MediaPicker":
                                                imageIds = FactoryRepository.Repositories.MediaRepository.UploadFile(path, file).ToString();
                                                break;
                                            case "Umbraco.MultipleMediaPicker":
                                                imageIds += (imageIds.Length > 0 ? "," : "") +
                                                            FactoryRepository.Repositories.MediaRepository.UploadFile(path, file);
                                                break;
                                            default:
                                                imageIds = FactoryRepository.Repositories.MediaRepository.UploadFile(path, file).ToString();
                                                break;
                                        }
                                    }
                                }
                                createdItem.SetValue(createdItem.PropertyTypes.Single(type =>
                                    type.Id == Convert.ToInt32(cmsField.TemplateField.FieldId)).Alias, imageIds);
                            }
                            else if (cmsField.Options != null && cmsField.Options.Any())
                            {
                                string selectedItems = string.Empty;
                                foreach (var option in cmsField.Options)
                                {
                                    if (dataSource.Select(x => x.Value.Value).Contains(option))
                                        selectedItems += (selectedItems.Length > 0 ? "," : "")
                                                        +
                                                        dataSource.Where(x => x.Value.Value == option)
                                                            .Select(x => x.Value.Id)
                                                            .First();
                                }
                                createdItem.SetValue(
                                    createdItem.PropertyTypes.Single(
                                        type => type.Id == Convert.ToInt32(cmsField.TemplateField.FieldId)).Alias,
                                    selectedItems);
                            }
                        }
                        break;
                    case "Umbraco.CheckBoxList":
                    case "Umbraco.DropDownMultiple":
                        {
                            string selectedItems = string.Empty;
                            foreach (var option in cmsField.Options)
                            {
                                if (dataSource.Select(x => x.Value.Value).Contains(option))
                                    selectedItems += (selectedItems.Length > 0 ? "," : "")
                                                    +
                                                    dataSource.Where(x => x.Value.Value == option)
                                                        .Select(x => x.Value.Id)
                                                        .First();
                            }
                            createdItem.SetValue(
                                createdItem.PropertyTypes.Single(
                                    type => type.Id == Convert.ToInt32(cmsField.TemplateField.FieldId)).Alias, selectedItems);
                        }
                        break;
                    case "Umbraco.MultiNodeTreePicker":
                        {
                            string selectedItems = string.Empty;
                            UmbracoHelper umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                            var rootNodes = umbracoHelper.TypedContentAtRoot();

                            foreach (var optionGC in cmsField.Options)
                            {
                                string option = System.Web.HttpUtility.HtmlDecode(optionGC.TrimEnd());
                                foreach (var rootNode in rootNodes)
                                {
                                    var node = rootNode.DescendantsOrSelf().Where(x => x.Name == option);
                                    if (node.Any())
                                    {
                                        selectedItems += (selectedItems.Length > 0 ? "," : "")
                                                             + node.First().Id;
                                    }
                                    else
                                    {
                                        var selectedNodes = SearchInChildrenNodes(rootNode, option);
                                        if (selectedNodes.Any())
                                        {
                                            selectedItems += (selectedItems.Length > 0 ? "," : "")
                                                                 + selectedNodes.First().Id;
                                        }
                                    }
                                }
                            }
                            
                            createdItem.SetValue(
                                createdItem.PropertyTypes.Single(
                                    type => type.Id == Convert.ToInt32(cmsField.TemplateField.FieldId)).Alias, selectedItems);
                        }
                        break;
                    default:

                        break;
                }
                ContextService.ContentService.Save(createdItem);
            }
            else
            {
                throw new KeyNotFoundException(String.Format("Some fields has been deleted from Document Type {0}", createdItem.ContentType.Name));
            }
        }

        private IEnumerable<IPublishedContent> SearchInChildrenNodes(IPublishedContent rootNode, string option)
        {
            IEnumerable<IPublishedContent> nodes = new List<IPublishedContent>();
            if (rootNode.Children.Any())
            {
                foreach (var childrenNode in rootNode.Children)
                {
                    var childNodes = childrenNode.DescendantsOrSelf().Where(x => x.Name == option);
                    if (childNodes.Any())
                    {
                        return childNodes;
                    }
                    else
                    {
                        SearchInChildrenNodes(childrenNode, option);
                    }
                }
            }
            return nodes;
        }


        public void MapFile(CmsItem item, CmsField cmsField)
        {
            IContent createdItem = ContextService.ContentService.GetById(Convert.ToInt32(item.Id));
            if (createdItem == null)
                return;

            var field = createdItem.PropertyTypes.SingleOrDefault(type =>
                    type.Id == Convert.ToInt32(cmsField.TemplateField.FieldId));
            if (field != null)
            {
                string imageIds = string.Empty;
                IDataTypeDefinition dataType =
                    ContextService.DataTypeService.GetDataTypeDefinitionById(field.DataTypeDefinitionId);

                if (cmsField.TemplateField != null)
                {
                    foreach (var file in cmsField.Files)
                    {
                        var fileName = file.FileName;
                        var ext = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
                        cmsField.TemplateField.FieldName = createdItem.PropertyTypes.Single(type =>
                                        type.Id == Convert.ToInt32(cmsField.TemplateField.FieldId)).Name;
                        var path = FactoryRepository.Repositories.MediaRepository.ResolveMediaPath(item, createdItem, cmsField);

                        if (!UmbracoConfig.For.UmbracoSettings().Content.DisallowedUploadFiles.Contains(ext))
                        {
                            switch (dataType.PropertyEditorAlias)
                            {
                                case "Umbraco.MediaPicker":
                                    imageIds = FactoryRepository.Repositories.MediaRepository.UploadFile(path, file).ToString();
                                    break;
                                case "Umbraco.MultipleMediaPicker":
                                    imageIds += (imageIds.Length > 0 ? "," : "") +
                                                FactoryRepository.Repositories.MediaRepository.UploadFile(path, file);
                                    break;
                                default:
                                    imageIds = FactoryRepository.Repositories.MediaRepository.UploadFile(path, file).ToString();
                                    break;
                            }
                        }
                    }
                    createdItem.SetValue(field.Alias, imageIds);
                    ContextService.ContentService.Save(createdItem);
                }

            }
            else
            {
                throw new KeyNotFoundException(String.Format("Some fields has been deleted from Document Type {0}", createdItem.ContentType.Name));
            }
        }

        public bool IfMappedItemExists(string itemId, CmsItem cmsItem, string mappingId, string gcPath)
        {
            if (itemId != null)
            {
                int intParentId;
                if (int.TryParse(itemId, out intParentId))
                {
                    var item =
                        ContextService.ContentService.GetChildren(intParentId)
                            .SingleOrDefault(x => x.Name == cmsItem.Title && x.GetValue("mappingId").ToString() == mappingId);
                    return
                        item != null;
                }
            }
            return false;
        }

        public bool IfMappedItemExists(string parentId, CmsItem cmsItem)
        {
            if (parentId != null)
            {
                int intParentId;
                if (int.TryParse(parentId, out intParentId))
                {
                    return
                        ContextService.ContentService.GetChildren(intParentId)
                            .SingleOrDefault(x => x.Name == cmsItem.Title) != null;
                }
            }
            return false;
        }

        public bool IfNotMappedItemExists(string parentId, CmsItem cmsItem)
        {
            if (parentId != null)
            {
                int intParentId;
                if (int.TryParse(parentId, out intParentId))
                {
                    return
                        ContextService.ContentService.GetChildren(intParentId)
                            .SingleOrDefault(x => x.Name == cmsItem.Title) != null;
                }
            }
            return false;
        }

        public string AddNewVersion(string itemId, CmsItem cmsItem, string mappingId, string gcPath)
        {
            if (!string.IsNullOrWhiteSpace(itemId))
            {
                return ContextService.ContentService.GetChildren(Convert.ToInt32(itemId))
                            .FirstOrDefault(item => item.Name == cmsItem.Title)?
                            .Id.ToString();
            }
            return null;
        }

        public string GetCmsItemLink(string host, string itemId)
        {
            int intItemId;
            if (int.TryParse(itemId, out intItemId))
            {
                //return ContextService.ContentService.GetById(intItemId)?.Path;
                return @"#/content/content/edit/" + intItemId;
            }
            return null;
        }

        public string GetItemId(string parentId, CmsItem cmsItem)
        {
            if (parentId != null)
            {
                int intParentId;
                if (int.TryParse(parentId, out intParentId))
                {
                    return
                        ContextService.ContentService.GetChildren(intParentId)
                            .SingleOrDefault(x => x.Name == cmsItem.Title)?
                            .Id.ToString();
                }
            }
            return null;
        }

        [Obsolete]
        private int UploadFile(File file, string mediaType, string itemName, string fieldName)
        {
            var uri = file.Url.StartsWith("http") ? file.Url : "https://gathercontent.s3.amazonaws.com/" + file.Url;
            var request = (HttpWebRequest)WebRequest.Create(uri);
            var resp = (HttpWebResponse)request.GetResponse();
            var stream = resp.GetResponseStream();

            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                stream?.CopyTo(memoryStream);

                if (memoryStream.Length > 0)
                {
                    var gatherContent = ContextService.MediaService.GetChildren(-1).FirstOrDefault(x => x.Name == "GatherContent");
                    if (gatherContent == null)
                    {
                        gatherContent = ContextService.MediaService.CreateMedia("GatherContent", -1, Constants.Conventions.MediaTypes.Folder);
                        ContextService.MediaService.Save(gatherContent);
                    }
                    var itemMedia =
                        ContextService.MediaService.GetChildren(gatherContent.Id).FirstOrDefault(x => x.Name == itemName);
                    if (itemMedia == null)
                    {
                        itemMedia = ContextService.MediaService.CreateMedia(itemName, gatherContent, Constants.Conventions.MediaTypes.Folder);
                        ContextService.MediaService.Save(itemMedia);
                    }
                    var fielfMedia =
                        ContextService.MediaService.GetChildren(itemMedia.Id).FirstOrDefault(x => x.Name == fieldName);
                    if (fielfMedia == null)
                    {
                        fielfMedia = ContextService.MediaService.CreateMedia(fieldName, itemMedia, Constants.Conventions.MediaTypes.Folder);
                        ContextService.MediaService.Save(fielfMedia);
                    }
                    var umbFile = ContextService.MediaService.GetChildren(fielfMedia.Id).FirstOrDefault(x => x.Name == file.FileName) ??
                                  ContextService.MediaService.CreateMedia(file.FileName, fielfMedia, mediaType);
                    umbFile.SetValue(Constants.Conventions.Media.File, file.FileName, memoryStream);
                    ContextService.MediaService.Save(umbFile);
                    return umbFile.Id;
                }

                return 0;
            }
        }
    }
}
