using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GatherContent.Connector.Entities;
using GatherContent.Connector.Entities.Entities;
using GatherContent.Connector.GatherContentService.Interfaces;
using GatherContent.Connector.GatherContentService.Services;
using GatherContent.Connector.IRepositories.Interfaces;
using GatherContent.Connector.IRepositories.Models.Import;
using GatherContent.Connector.IRepositories.Models.Mapping;
using GatherContent.Connector.Managers.Enums;
using GatherContent.Connector.Managers.Interfaces;
using GatherContent.Connector.Managers.Models.ImportItems;
using GatherContent.Connector.Managers.Models.Mapping;
using GatherContent.Connector.Managers.Models.UpdateItems;
using GatherContent.Connector.Managers.Models.ImportItems.New;
using GatherContent.Connector.IRepositories;
using GatherContent.Connector.UmbracoManagers.IoC;
using GatherContent.Connector.UmbracoRepositories.IoC;
using GatherContent.Connector.UmbracoRepositories.Repositories;

namespace GatherContent.Connector.Managers.Managers
{
    /// <summary>
    /// 
    /// </summary>
    public class ImportManager : BaseManager, IImportManager
    {

        #region Utilities

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private string ConvertMsecToFormattedDate(double date)
        {
            var posixTime = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
            var dateFormat = FactoryService.CurrentSettings.DateFormat;
            if (string.IsNullOrEmpty(dateFormat))
            {
                dateFormat = Constants.DateFormat;
            }
            var gcUpdateDate =
                posixTime.AddMilliseconds(date * 1000).ToString(dateFormat);
            return gcUpdateDate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="templates"></param>
        /// <returns></returns>
        private List<MappingResultModel> TryMapItems(List<GCItem> items, List<TemplateMapping> templates)
        {
            var result = new List<MappingResultModel>();
            var templatesDictionary = new Dictionary<int, GCTemplate>();

            foreach (GCItem gcItem in items)
            {
                GCTemplate gcTemplate = GetTemplate(gcItem.TemplateId.Value, templatesDictionary);

                MappingResultModel cmsItem;
                TryMapItem(gcItem, gcTemplate, templates, out cmsItem);
                result.Add(cmsItem);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="templatesDictionary"></param>
        /// <returns></returns>
        private GCTemplate GetTemplate(int templateId, Dictionary<int, GCTemplate> templatesDictionary)
        {
            GCTemplate gcTemplate;
            templatesDictionary.TryGetValue(templateId, out gcTemplate);
            if (gcTemplate == null)
            {
                gcTemplate = FactoryService.Services.TemplateService.GetSingleTemplate(templateId.ToString()).Data;
                templatesDictionary.Add(templateId, gcTemplate);
            }

            return gcTemplate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gcItem"></param>
        /// <param name="gcTemplate"></param>
        /// <param name="selectedMappingId"></param>
        /// <param name="result"></param>
        /// <param name="selectedLocationId"></param>
        private void TryMapItem(GCItem gcItem, GCTemplate gcTemplate, string selectedMappingId, out MappingResultModel result, string selectedLocationId = null)
        {
            bool isUpdate = gcItem is UpdateGCItem;

            List<Element> gcFields = gcItem.Config.SelectMany(i => i.Elements).ToList();
            var template = FactoryRepository.Repositories.MappingRepository.GetMappingById(selectedMappingId);


            if (template == null)
            {
                string errorMessage = isUpdate ? "Update failed: Template not mapped" : "Import failed: Template not mapped";
                result = new MappingResultModel(gcItem, null, gcTemplate.Name, null, string.Empty, errorMessage, false, selectedLocationId);
                return;
            }

            List<ImportCMSField> fields;

            IEnumerable<IGrouping<string, FieldMapping>> groupedFields = template.FieldMappings.GroupBy(i => i.CmsField.TemplateField.FieldId);

            var files = new List<File>();
            if (gcItem.Config.SelectMany(config => config.Elements).Select(element => element.Type).Contains("files"))
            {

                foreach (var file in FactoryService.Services.ItemsService.GetItemFiles(gcItem.Id.ToString()).Data)
                {
                    files.Add(new File
                    {
                        FileName = file.FileName,
                        Url = file.Url,

                        UpdatedDate = file.Updated
                    });


                }
            }


            TryMapItemState mapState = TryMapFields(gcFields, groupedFields, files, out fields);
            if (mapState == TryMapItemState.FieldError)
            {
                string errorMessage = isUpdate ? "Update failed: Template fields mismatch" : "Import failed: Template fields mismatch";
                result = new MappingResultModel(gcItem, null, gcTemplate.Name, null, string.Empty, errorMessage, false, selectedLocationId);
                return;
            }

            string cmsId = string.Empty;
            string message = "Import Successful";
            if (isUpdate)
            {
                cmsId = (gcItem as UpdateGCItem).CMSId;
                message = "Update Successful";
            }

            result = new MappingResultModel(gcItem, fields, gcTemplate.Name, template.CmsTemplate.TemplateId, cmsId, message, true, selectedLocationId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="gcTemplate"></param>
        /// <param name="templates"></param>
        /// <param name="result"></param>
        private void TryMapItem(GCItem item, GCTemplate gcTemplate, List<TemplateMapping> templates, out MappingResultModel result)
        {
            bool isUpdate = item is UpdateGCItem;

            List<Element> gcFields = item.Config.SelectMany(i => i.Elements).ToList();

            TemplateMapping template;
            TryMapItemState templateMapState = TryGetTemplate(templates, item.TemplateId.ToString(), out template);

            if (templateMapState == TryMapItemState.TemplateError)
            {
                string errorMessage = isUpdate ? "Update failed: Template not mapped" : "Import failed: Template not mapped";
                result = new MappingResultModel(item, null, gcTemplate.Name, null, string.Empty, errorMessage, false);
                return;
            }

            List<ImportCMSField> fields;
            IEnumerable<IGrouping<string, FieldMapping>> groupedFields = template.FieldMappings.GroupBy(i => i.CmsField.TemplateField.FieldId);

            var files = new List<File>();
            if (item.Config.SelectMany(config => config.Elements).Select(element => element.Type).Contains("files"))
            {

                foreach (var file in FactoryService.Services.ItemsService.GetItemFiles(item.Id.ToString()).Data)
                {
                    files.Add(new File
                    {
                        FileName = file.FileName,
                        Url = file.Url,
                        UpdatedDate = file.Updated
                    });


                }
            }


            TryMapItemState mapState = TryMapFields(gcFields, groupedFields, files, out fields);
            if (mapState == TryMapItemState.FieldError)
            {
                string errorMessage = isUpdate ? "Update failed: Template fields mismatch" : "Import failed: Template fields mismatch";
                result = new MappingResultModel(item, null, gcTemplate.Name, null, string.Empty, errorMessage, false);
                return;
            }

            string cmsId = string.Empty;
            string message = "Import Successful";
            if (isUpdate)
            {
                cmsId = (item as UpdateGCItem).CMSId;
                message = "Update Successful";
            }

            result = new MappingResultModel(item, fields, gcTemplate.Name, template.CmsTemplate.TemplateId, cmsId, message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gcFields"></param>
        /// <param name="fieldsMappig"></param>
        /// <param name="files"></param>
        /// <param name="importCMSField"></param>
        /// <returns></returns>
        private TryMapItemState TryMapField(List<Element> gcFields, IGrouping<string, FieldMapping> fieldsMappig, List<File> files, out ImportCMSField importCMSField)
        {
            var cmsFieldName = fieldsMappig.Key;

            var gcFieldsForMapping = GetFieldsForMapping(fieldsMappig, gcFields);


            var field = gcFieldsForMapping.FirstOrDefault();

            if (field == null)
            {
                importCMSField = new ImportCMSField(string.Empty, cmsFieldName, null, string.Empty, null, null);
                return TryMapItemState.FieldError;
            }


            if (IsMappedFieldsHaveDifrentTypes(gcFieldsForMapping))
            {
                importCMSField = new ImportCMSField(string.Empty, cmsFieldName, field.Label, string.Empty, null, null);
                return TryMapItemState.FieldError;
            }

            var value = GetValue(gcFieldsForMapping);
            var options = GetOptions(gcFieldsForMapping);
            //files = files.Where(item => item.FieldId == field.Name).ToList();
            //TODO: used new List<Option>() here to build; will be removed soon
            importCMSField = new ImportCMSField(field.Type, cmsFieldName, field.Label, value.ToString(), new List<Option>(), files);

            return TryMapItemState.Success;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templates"></param>
        /// <param name="templateId"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private TryMapItemState TryGetTemplate(List<TemplateMapping> templates, string templateId, out TemplateMapping result)
        {
            if (templates == null)
            {
                result = null;
                return TryMapItemState.TemplateError;
            }

            result = templates.FirstOrDefault(i => templateId == i.GcTemplate.GcTemplateId);
            if (result == null)
                return TryMapItemState.TemplateError;

            return TryMapItemState.Success;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gcFields"></param>
        /// <param name="fieldsMappig"></param>
        /// <param name="files"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private TryMapItemState TryMapFields(List<Element> gcFields, IEnumerable<IGrouping<string, FieldMapping>> fieldsMappig, List<File> files, out List<ImportCMSField> result)
        {
            result = new List<ImportCMSField>();
            foreach (IGrouping<string, FieldMapping> grouping in fieldsMappig)
            {
                ImportCMSField cmsField;
                TryMapItemState mapState = TryMapField(gcFields, grouping, files, out cmsField);
                if (mapState == TryMapItemState.FieldError)
                    return mapState;
                result.Add(cmsField);
            }

            return TryMapItemState.Success;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        private object GetValue(IEnumerable<Element> fields)
        {
            string value = string.Join("", fields.Select(i => i.Value));
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        private List<string> GetOptions(IEnumerable<Element> fields)
        {
            var result = new List<string>();
            foreach (Element field in fields)
            {
                if (field.Options != null)
                    result.AddRange(field.Options.Where(x => x.Selected).Select(x => x.Label));
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        private bool IsMappedFieldsHaveDifrentTypes(List<Element> fields)
        {
            return fields.Select(i => i.Type).Distinct().Count() > 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldsMappig"></param>
        /// <param name="gcFields"></param>
        /// <returns></returns>
        private List<Element> GetFieldsForMapping(IGrouping<string, FieldMapping> fieldsMappig, List<Element> gcFields)
        {
            IEnumerable<string> gsFiledNames = fieldsMappig.Select(i => i.GcField.Id);
            IEnumerable<Element> gcFieldsForMapping = gcFields.Where(i => gsFiledNames.Contains(i.Name));

            return gcFieldsForMapping.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projects"></param>
        /// <param name="projectIdStr"></param>
        /// <returns></returns>
        private Project GetProject(List<Project> projects, string projectIdStr)
        {
            int projectId;
            int.TryParse(projectIdStr, out projectId);

            Project project = projectId != 0 ? projects.FirstOrDefault(i => i.Id == projectId) : projects.FirstOrDefault();

            return project;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        private List<GCStatus> GetStatuses(int projectId)
        {
            StatusesEntity statuses = FactoryService.Services.ProjectsService.GetAllStatuses(projectId.ToString());
            return statuses.Data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        private List<GCItem> GetItems(int projectId)
        {
            ItemsEntity items = FactoryService.Services.ItemsService.GetItems(projectId.ToString());
            return items.Data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        private List<GCTemplate> GetTemplates(int projectId)
        {
            return GetTemplates(projectId.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        private List<GCTemplate> GetTemplates(string projectId)
        {
            TemplatesEntity templates = FactoryService.Services.TemplateService.GetTemplates(projectId);
            return templates.Data;
        }

        private string GetBreadcrumb(GCItem item, List<GCItem> items)
        {
            var names = new List<string>();
            string result = BuildBreadcrumb(item, items, names);
            return result;
        }

        private string BuildBreadcrumb(GCItem item, List<GCItem> items, List<string> names)
        {
            names.Add(item.Name);

            if (item.ParentId != 0)
            {
                GCItem next = items.FirstOrDefault(i => i.Id == item.ParentId);
                return BuildBreadcrumb(next, items, names);
            }

            names.Reverse();

            string url = string.Join("/", names);

            return string.Format("/{0}", url);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="templates"></param>
        /// <returns></returns>
        private List<ImportListItem> MapItems(List<GCItem> items, List<GCTemplate> templates)
        {
            var mappedItems = items.Where(i => i.TemplateId != null).ToList();
            var dateFormat = FactoryService.CurrentSettings.DateFormat;
            if (string.IsNullOrEmpty(dateFormat))
            {
                dateFormat = Constants.DateFormat;
            }

            var result = new List<ImportListItem>();
            foreach (var mappedItem in mappedItems)
            {
                var mappings = FactoryRepository.Repositories.MappingRepository.GetMappingsByGcTemplateId(mappedItem.TemplateId.ToString());
                var availableMappings = mappings.Select(availableMappingModel => new AvailableMapping
                {
                    Id = availableMappingModel.MappingId,
                    Title = availableMappingModel.MappingTitle,
                }).ToList();

                result.Add(new ImportListItem(mappedItem, templates.FirstOrDefault(templ => templ.Id == mappedItem.TemplateId), items, dateFormat, availableMappings));
            }

            return result.ToList();
        }


        private List<ItemModel> MapImportItems(List<GCItem> items, List<GCTemplate> templates)
        {
            var model = new List<ItemModel>();
            var mappedItems = items.Where(i => i.TemplateId != null).ToList();
            var dateFormat = FactoryService.CurrentSettings.DateFormat;
            if (string.IsNullOrEmpty(dateFormat))
            {
                dateFormat = Constants.DateFormat;
            }


            foreach (var mappedItem in mappedItems)
            {
                var template = templates.FirstOrDefault(templ => templ.Id == mappedItem.TemplateId);
                var mappings = FactoryRepository.Repositories.MappingRepository.GetMappingsByGcTemplateId(mappedItem.TemplateId.ToString());
                var availableMappings = mappings.Select(availableMappingModel => new AvailableMapping
                {
                    Id = availableMappingModel.MappingId,
                    Title = availableMappingModel.MappingTitle,
                    DefaultLocationId = availableMappingModel.DefaultLocationId,
                    DefaultLocationTitle = availableMappingModel.DefaultLocationTitle,
                    CmsTemplateName = availableMappingModel.CmsTemplate.TemplateName

                }).ToList();

                model.Add(new ItemModel
                {
                    GcItem = new GcItemModel
                    {
                        Id = mappedItem.Id.ToString(),
                        Title = mappedItem.Name,
                        LastUpdatedInGc = mappedItem.Updated.Date.ToLocalTime().ToString(dateFormat)
                    },
                    GcTemplate = new GcTemplateModel
                    {
                        Name = template != null ? template.Name : "",
                        Id = template != null ? template.Id.ToString() : ""
                    },
                    Status = new GcStatusModel
                    {
                        Id = mappedItem.Status.Data.Id,
                        Name = mappedItem.Status.Data.Name,
                        Color = mappedItem.Status.Data.Color
                    },
                    AvailableMappings = new AvailableMappings
                    {
                        Mappings = availableMappings
                    },
                    Breadcrumb = GetBreadcrumb(mappedItem, items),
                });
            }
            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="templates"></param>
        /// <returns></returns>
        private List<ImportItembyLocation> MapItemsByLocation(List<GCItem> items, List<GCTemplate> templates)
        {
            var mappedItems = items.Where(i => i.TemplateId != null).ToList();
            var dateFormat = FactoryService.CurrentSettings.DateFormat;
            if (string.IsNullOrEmpty(dateFormat))
            {
                dateFormat = Constants.DateFormat;
            }

            var result = new List<ImportItembyLocation>();
            foreach (var mappedItem in mappedItems)
            {
                var mappings = FactoryRepository.Repositories.MappingRepository.GetMappingsByGcTemplateId(mappedItem.TemplateId.ToString());
                var availableMappings = mappings.Select(availableMappingModel => new AvailableMappingByLocation
                {
                    Id = availableMappingModel.MappingId,
                    Title = availableMappingModel.MappingTitle,
                    OpenerId = "drop-tree" + Guid.NewGuid(),
                    ScTemplate = availableMappingModel.CmsTemplate.TemplateId,
                    IsShowing = false,
                    DefaultLocation = availableMappingModel.DefaultLocationId,
                    DefaultLocationTitle = availableMappingModel.DefaultLocationTitle,
                }).ToList();

                result.Add(new ImportItembyLocation(mappedItem, templates.FirstOrDefault(templ => templ.Id == mappedItem.TemplateId), items, dateFormat, availableMappings));
            }

            return result.ToList();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="importResult"></param>
        /// <returns></returns>
        private List<MappingResultModel> GetSuccessfulImportedItems(List<MappingResultModel> importResult)
        {
            return importResult.Where(i => i.IsImportSuccessful).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="statusId"></param>
        /// <param name="projectId"></param>
        private void PostNewStatusesForItems(List<MappingResultModel> items, string statusId, string projectId)
        {
            foreach (MappingResultModel item in items)
            {
                FactoryService.Services.ItemsService.ChooseStatusForItem(item.GCItemId, statusId);
                var status = FactoryService.Services.ProjectsService.GetSingleStatus(statusId, projectId);
                item.Status.Color = status.Data.Color;
                item.Status.Name = status.Data.Name;
            }
        }

        private GcStatusModel PostNewItemStatus(string gcItemId, string statusId, string projectId)
        {
            FactoryService.Services.ItemsService.ChooseStatusForItem(gcItemId, statusId);
            var status = FactoryService.Services.ProjectsService.GetSingleStatus(statusId, projectId);
            var statusModel = new GcStatusModel { Color = status.Data.Color, Name = status.Data.Name };
            return statusModel;
        }

        #endregion




        public List<ItemModel> GetImportDialogModel(string itemId, string projectId)
        {
            var model = new List<ItemModel>();
            if (projectId == "0") return null;

            var project = GetGcProjectEntity(projectId);

            if (project != null)
            {
                List<GCTemplate> templates = GetTemplates(project.Data.Id);

                List<GCItem> items = GetItems(project.Data.Id);
                items = items.OrderBy(item => item.Status.Data.Name).ToList();
                model = MapImportItems(items, templates);
                model =
                    model.Where(x => x.AvailableMappings?.Mappings != null && x.AvailableMappings.Mappings.Count > 0)
                        .ToList();
                return model;
            }

            return model;
        }




        public GatherContent.Connector.Managers.Models.ImportItems.New.FiltersModel GetFilters(string projectId)
        {
            var items = GetImportDialogModel(null, projectId);

            Account account = GetAccount();

            List<Project> gcProjects = GetProjects(account.Id);

            var projects = new List<GcProjectModel>();
            foreach (var gcProject in gcProjects)
            {
                projects.Add(new GcProjectModel
                {
                    Id = gcProject.Id.ToString(),
                    Name = gcProject.Name
                });
            }

            if (projectId != "0")
            {
                Project gcProject = GetProject(gcProjects, projectId);

                List<GCTemplate> gcTemplates = GetTemplates(gcProject.Id);
                var templates = new List<GcTemplateModel>();
                foreach (var gcTemplate in gcTemplates)
                {
                    if (items.Any(x => x.GcTemplate.Id == gcTemplate.Id.ToString()))
                        templates.Add(new GcTemplateModel
                        {
                            Id = gcTemplate.Id.ToString(),
                            Name = gcTemplate.Name

                        });
                }
                List<GCStatus> gcStatuses = GetStatuses(gcProject.Id);

                var statuses = new List<GcStatusModel>();
                foreach (var gcStatus in gcStatuses)
                {
                    //if (items.Any(x => x.Status.Id == gcStatus.Id.ToString()))
                        statuses.Add(new GcStatusModel
                        {
                            Id = gcStatus.Id.ToString(),
                            Name = gcStatus.Name,
                            Color = gcStatus.Color
                        });
                }


                return new GatherContent.Connector.Managers.Models.ImportItems.New.FiltersModel
                {
                    CurrentProject = new GcProjectModel
                    {
                        Id = gcProject.Id.ToString(),
                        Name = gcProject.Name
                    },
                    Projects = projects,
                    Statuses = statuses,
                    Templates = templates
                };

            }

            return new GatherContent.Connector.Managers.Models.ImportItems.New.FiltersModel
            {
                Projects = projects
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="items"></param>
        /// <param name="projectId"></param>
        /// <param name="statusId"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        private List<ItemResultModel> Import(string itemId, List<ImportItemModel> items, string projectId, string statusId, string language)
        {
            var model = new List<ItemResultModel>();

            //get all paths
            var fullGcPaths = GetItemsMap(projectId, items.Select(x => x.Id));
            var pathItemsToBeRemoved = new List<int>();

            Dictionary<string, List<ItemEntity>> shortPaths = new Dictionary<string, List<ItemEntity>>();
            if (fullGcPaths.Count() > 1)
            {
                var firstPath = fullGcPaths.First();
                foreach (var path in firstPath.Value)
                {
                    //if all paths start with same item and this item is not selected
                    //TODO: check all cases work: add UT
                    if (fullGcPaths.Select(x => x.Value).All(x => x.First().Data.Id == path.Data.Id) && !items.Select(x => x.Id).Contains(path.Data.Id.ToString()))
                    {
                        pathItemsToBeRemoved.Add(path.Data.Id);
                    }
                }
            }

            foreach (var item in fullGcPaths)
            {
                List<ItemEntity> itemsToAdd = new List<ItemEntity>();

                foreach (var gcPathItem in item.Value)
                {
                    if (!pathItemsToBeRemoved.Contains(gcPathItem.Data.Id))
                    {
                        itemsToAdd.Add(gcPathItem);
                    }
                }
                shortPaths.Add(item.Key, itemsToAdd);
            }

            var sorted = shortPaths.OrderBy(x => x.Value.Count).ThenBy(x => x.Value.First().Data.Name);//sort to start from shortest and alphabetically asc

            foreach (var path in sorted)
            {
                var itemResponseModel = new ItemResultModel
                {
                    IsImportSuccessful = true,
                    ImportMessage = "Import Successful"
                };

                var gcItem = path.Value.Last(); //this is the item we selected to import
                var item = items.FirstOrDefault(x => x.Id == gcItem.Data.Id.ToString()); //item coming from UI; selected mapping and other info

                if (gcItem != null && gcItem.Data != null && gcItem.Data.TemplateId != null)
                {
                    if (!string.IsNullOrEmpty(FactoryService.CurrentSettings.GatherContentUrl))
                    {
                        itemResponseModel.GcLink = string.Concat(FactoryService.CurrentSettings.GatherContentUrl, "/item/", gcItem.Data.Id);
                    }
                    itemResponseModel.GcItem = new GcItemModel
                    {
                        Id = gcItem.Data.Id.ToString(),
                        Title = gcItem.Data.Name
                    };

                    itemResponseModel.Status = new GcStatusModel
                    {
                        Color = gcItem.Data.Status.Data.Color,
                        Name = gcItem.Data.Status.Data.Name,
                    };

                    var gcTemplate = FactoryService.Services.TemplateService.GetSingleTemplate(gcItem.Data.TemplateId.ToString());
                    itemResponseModel.GcTemplate = new GcTemplateModel
                    {
                        Id = gcTemplate.Data.Id.ToString(),
                        Name = gcTemplate.Data.Name
                    };

                    //element that corresponds to item in CMS that holds mappings
                    TemplateMapping templateMapping = FactoryRepository.Repositories.MappingRepository.GetMappingById(item.SelectedMappingId);

                    List<Element> gcFields = gcItem.Data.Config.SelectMany(i => i.Elements).ToList();

                    if (templateMapping != null) // template found, now map fields here
                    {
                        var files = new List<File>();
                        if (gcItem.Data.Config.SelectMany(config => config.Elements).Select(element => element.Type).Contains("files"))
                        {
                            foreach (var file in FactoryService.Services.ItemsService.GetItemFiles(gcItem.Data.Id.ToString()).Data)
                            {
                                files.Add(new File
                                {
                                    FileName = file.FileName,
                                    Url = file.Url,
                                    FieldId = file.Field,
                                    UpdatedDate = file.Updated
                                });
                            }
                        }

                        bool fieldError = false;

                        var groupedFields = templateMapping.FieldMappings.GroupBy(i => i.CmsField);

                        foreach (var grouping in groupedFields)
                        {
                            CmsField cmsField = grouping.Key;

                            var gcFieldIds = grouping.Select(i => i.GcField.Id);
                            var gcFieldsToMap = grouping.Select(i => i.GcField);

                            IEnumerable<Element> gcFieldsForMapping = gcFields.Where(i => gcFieldIds.Contains(i.Name)).ToList();

                            var gcField = gcFieldsForMapping.FirstOrDefault();

                            if (gcField != null)
                            {
                                var value = GetValue(gcFieldsForMapping);
                                var options = GetOptions(gcFieldsForMapping);

                                cmsField.Files = files.Where(x => x.FieldId == gcField.Name).ToList();
                                cmsField.Value = value;
                                cmsField.Options = options;

                                //update GC fields' type
                                foreach (var field in gcFieldsToMap)
                                {
                                    field.Type = gcField.Type;
                                }
                            }
                            else
                            {
                                //if field error, set error message
                                itemResponseModel.ImportMessage = "Import failed: Template fields mismatch";
                                itemResponseModel.IsImportSuccessful = false;
                                fieldError = true;
                                break;
                            }
                        }

                        if (!fieldError)
                        {
                            var cmsContentIdField = new FieldMapping
                            {
                                CmsField = new CmsField
                                {
                                    TemplateField = new CmsTemplateField { FieldName = "GC Content Id" },
                                    Value = gcItem.Data.Id.ToString()
                                }
                            };
                            templateMapping.FieldMappings.Add(cmsContentIdField);

                            var cmsItem = new CmsItem
                            {
                                Template = templateMapping.CmsTemplate,
                                Title = gcItem.Data.Name,
                                Fields = templateMapping.FieldMappings.Select(x => x.CmsField).ToList(),
                                Language = language
                            };

                            var gcPath = string.Join("/", path.Value.Select(x => x.Data.Name));

                            var parentId = itemId;

                            bool alreadyMappedItemInPath = false;
                            //for each mapping which is fact GC Item => Sitecore/Umbraco item - get GC Path and run through its each item
                            for (int i = 0; i < path.Value.Count; i++)
                            {
                                //for each path item check if it exists already in CMS and if yes - skip; otherwise - add not mapped item
                                if (i == path.Value.Count - 1)
                                {
                                    //if we at the last item in the path - import mapped item
                                    if (FactoryRepository.Repositories.ItemsRepository.IfMappedItemExists(parentId, cmsItem, templateMapping.MappingId, gcPath))
                                    {
                                        cmsItem.Id = FactoryRepository.Repositories.ItemsRepository.AddNewVersion(parentId, cmsItem, templateMapping.MappingId, gcPath);
                                    }
                                    else
                                    {
                                        cmsItem.Id = FactoryRepository.Repositories.ItemsRepository.CreateMappedItem(parentId, cmsItem, templateMapping.MappingId, gcPath);
                                    }
                                    parentId = cmsItem.Id;
                                    //

                                    var fields = templateMapping.FieldMappings;
                                    try
                                    {
                                        foreach (var field in fields)
                                        {
                                            if (field.GcField != null)
                                            {
                                                switch (field.GcField.Type)
                                                {
                                                    case "choice_radio":
                                                    case "choice_checkbox":
                                                    {
                                                            FactoryRepository.Repositories.ItemsRepository.MapChoice(cmsItem, field.CmsField);
                                                    }
                                                        break;
                                                    case "files":
                                                    {
                                                            FactoryRepository.Repositories.ItemsRepository.MapFile(cmsItem, field.CmsField);
                                                    }
                                                        break;
                                                    default:
                                                    {
                                                            FactoryRepository.Repositories.ItemsRepository.MapText(cmsItem, field.CmsField);
                                                    }
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    catch (KeyNotFoundException ex)
                                    {
                                        itemResponseModel.ImportMessage = "Import failed: Some fields has been deleted from CMS Template";
                                        itemResponseModel.IsImportSuccessful = false;
                                        break;
                                    }
                                    //set CMS link after we got out CMS Id
                                    var cmsLink = FactoryRepository.Repositories.ItemsRepository.GetCmsItemLink(HttpContext.Current.Request.Url.Host, cmsItem.Id);
                                    itemResponseModel.CmsLink = cmsLink;


                                    if (!string.IsNullOrEmpty(statusId))
                                    {
                                        var status = PostNewItemStatus(gcItem.Data.Id.ToString(), statusId, projectId);
                                        if (itemResponseModel == null) continue;
                                        itemResponseModel.Status.Color = status.Color;
                                        itemResponseModel.Status.Name = status.Name;
                                    }
                                }
                                else
                                {
                                    var currentCmsItem = new CmsItem
                                    {
                                        Title = path.Value[i].Data.Name,
                                        Language = language
                                    };
                                    //if we are not at the selected item, somewhere in the middle
                                    //1. если замапленный айтем существует (такое же название и такой же gc path?), то тогда выставляем alreadyMappedItemInPath = тру
                                    //и пропускаем всякое создание, сеттим только парент id
                                    //2. иначе если есть незамапленный айтем:
                                    // - alreadyMappedItemInPath == true = - скипуем создание, выставляем его как парент айди
                                    // - alreadyMappedItemInPath == false, - скипуем всё, парент айди не меняем
                                    //3. айтема никакого нет
                                    // - alreadyMappedItemInPath == true = - создаём незамапленный айтем, выставляем его как парент айди
                                    // - alreadyMappedItemInPath == false, - скипуем всё, парент айди не меняем
                                    if (FactoryRepository.Repositories.ItemsRepository.IfMappedItemExists(parentId, currentCmsItem))
                                    {
                                        //cmsItem.Id = ItemsRepository.CreateNotMappedItem(parentId, notMappedCmsItem);
                                        //parentId = cmsItem.Id;
                                        alreadyMappedItemInPath = true;
                                        parentId = FactoryRepository.Repositories.ItemsRepository.GetItemId(parentId, currentCmsItem);
                                    }
                                    else if (FactoryRepository.Repositories.ItemsRepository.IfNotMappedItemExists(parentId, currentCmsItem))
                                    {
                                        if (alreadyMappedItemInPath)
                                        {
                                            parentId = FactoryRepository.Repositories.ItemsRepository.GetItemId(parentId, currentCmsItem);
                                        }
                                    }
                                    else
                                    {
                                        if (alreadyMappedItemInPath)
                                        {
                                            parentId = FactoryRepository.Repositories.ItemsRepository.CreateNotMappedItem(parentId, currentCmsItem);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //no template mapping, set error message
                        itemResponseModel.ImportMessage = "Import failed: Template not mapped";
                        itemResponseModel.IsImportSuccessful = false;
                    }
                }
                model.Add(itemResponseModel);
            }

            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="items"></param>
        /// <param name="projectId"></param>
        /// <param name="statusId"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public List<ItemResultModel> ImportItems(string itemId, List<ImportItemModel> items, string projectId, string statusId, string language)
        {
            return Import(itemId, items, projectId, statusId, language);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="gcItemIds"></param>
        /// <returns></returns>
        public Dictionary<string, List<ItemEntity>> GetItemsMap(string projectId, IEnumerable<string> gcItemIds)
        {
            List<ItemEntity> items = new List<ItemEntity>();
            Dictionary<string, List<ItemEntity>> paths = new Dictionary<string, List<ItemEntity>>();
            var accounts = FactoryService.Services.AccountsService.GetAccounts();
            var account = accounts.Data.FirstOrDefault(acc => acc.Slug.ToLower() == FactoryService.CurrentSettings.TenantName);

            if (account != null)
            {
                var project = FactoryService.Services.ProjectsService.GetProjects(account.Id).Data.FirstOrDefault(p => p.Active && p.Id.ToString() == projectId);

                if (project != null)
                {
                    foreach (var gcItemId in gcItemIds)
                    {
                        string itemInPathId = gcItemId;
                        ItemEntity item = null;
                        List<ItemEntity> path = new List<ItemEntity>();
                        while (true)
                        {
                            if (items.All(x => x.Data.Id.ToString() != itemInPathId)) //if we've not requested it yet
                            {
                                var gcItem = FactoryService.Services.ItemsService.GetSingleItem(itemInPathId);
                                if (gcItem != null)
                                {
                                    item = gcItem;
                                    items.Add(item);
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                                item = items.First(x => x.Data.Id.ToString() == itemInPathId);
                            }


                            if (item != null)
                            {
                                path.Add(item);
                                if (item.Data.ParentId != 0)
                                {
                                    itemInPathId = item.Data.ParentId.ToString();
                                    continue;
                                }
                            }
                            break;
                        }
                        path.Reverse();
                        paths.Add(gcItemId, path);
                    }
                    return paths;
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="projectId"></param>
        /// <param name="statusId"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public List<ItemResultModel> ImportItemsWithLocation(List<LocationImportItemModel> items, string projectId, string statusId, string language)
        {
            var importItems = new List<ImportItemModel>();

            if (items == null) return null;

            foreach (var item in items)
            {
                if (item.IsImport)
                {
                    importItems.Add(new ImportItemModel
                    {
                        Id = item.Id,
                        SelectedMappingId = item.SelectedMappingId,
                        DefaultLocation = item.SelectedLocation
                    });
                }
            }

            var result = new List<ItemResultModel>();

            var groupByLocation = importItems.GroupBy(x => x.DefaultLocation);

            foreach (var locationGroup in groupByLocation)
            {
                var importedItems = ImportItems(locationGroup.Key, locationGroup.ToList(), projectId, statusId, language);
                result.AddRange(importedItems);
            }

            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public List<MappingResultModel> MapItems(List<GCItem> items)
        {
            var templates = FactoryRepository.Repositories.MappingRepository.GetMappings();
            List<MappingResultModel> result = TryMapItems(items, templates);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public List<MappingResultModel> MapItems(List<ImportItemModel> items)
        {
            var result = new List<MappingResultModel>();
            var templatesDictionary = new Dictionary<int, GCTemplate>();

            foreach (var importItem in items)
            {
                var gcItem = FactoryService.Services.ItemsService.GetSingleItem(importItem.Id);

                if (gcItem != null && gcItem.Data != null && gcItem.Data.TemplateId != null)
                {
                    GCTemplate gcTemplate = GetTemplate(gcItem.Data.TemplateId.Value, templatesDictionary);

                    MappingResultModel cmsItem;
                    TryMapItem(gcItem.Data, gcTemplate, importItem.SelectedMappingId, out cmsItem, importItem.DefaultLocation);
                    result.Add(cmsItem);
                }
            }

            return result;
        }
    }
}
