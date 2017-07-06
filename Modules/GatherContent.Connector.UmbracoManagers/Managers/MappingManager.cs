using System;
using System.Collections.Generic;
using System.Linq;
using GatherContent.Connector.Entities;
using GatherContent.Connector.GatherContentService.Interfaces;
using GatherContent.Connector.GatherContentService.Services;
using GatherContent.Connector.IRepositories.Interfaces;
using GatherContent.Connector.IRepositories.Models.Import;
using GatherContent.Connector.IRepositories.Models.Mapping;
using GatherContent.Connector.Managers.Interfaces;
using GatherContent.Connector.Managers.Models.Mapping;
using GatherContent.Connector.IRepositories;
using GatherContent.Connector.UmbracoManagers.IoC;
using GatherContent.Connector.UmbracoRepositories.IoC;
using GatherContent.Connector.UmbracoRepositories.Repositories;

namespace GatherContent.Connector.Managers.Managers
{
    /// <summary>
    /// 
    /// </summary>
    public class MappingManager : BaseManager, IMappingManager
    {
        #region Constants
        public const string FieldGcContentId = "{955A4DD9-6A01-458E-9791-3C99F5E076A8}";
        public const string FieldLastSyncDate = "{F9D2EA57-86A2-45CF-9C28-8D8CA72A2669}";
        #endregion


        #region Utilities

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private DateTime ConvertMsecToDate(double date)
        {
            var posixTime = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
            var gcUpdateDate =
                posixTime.AddMilliseconds(date * 1000);
            return gcUpdateDate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scTemplates"></param>
        /// <returns></returns>
        private IEnumerable<CmsTemplateModel> MapCmsTemplates(IEnumerable<CmsTemplate> scTemplates)
        {
            var templates = new List<CmsTemplateModel>();

            foreach (var cmsTemplate in scTemplates)
            {
                var templateModel = new CmsTemplateModel
                {
                    Name = cmsTemplate.TemplateName,
                    Id = cmsTemplate.TemplateId
                };

                foreach (var field in cmsTemplate.TemplateFields)
                {
                    if (field.FieldId != FieldGcContentId &&
                        field.FieldId != FieldLastSyncDate)
                    {
                        var scField = new CmsTemplateFieldModel
                        {
                            Name = field.FieldName,
                            Id = field.FieldId,
                            Type = field.FieldType

                        };
                        templateModel.Fields.Add(scField);
                    }
                }
                templates.Add(templateModel);
            }
            return templates;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateMapping"></param>
        /// <returns></returns>
        private MappingModel MapAddMappingModel(TemplateMapping templateMapping)
        {
            var addCmsMappingModel = new MappingModel
            {
                GcTemplate = new GcTemplateModel
                {
                    Id = templateMapping.GcTemplate.GcTemplateId,
                },
                CmsTemplate = new CmsTemplateModel
                {
                    Id = templateMapping.CmsTemplate.TemplateId,
                },
                MappingTitle = templateMapping.MappingTitle,
                DefaultLocationId = templateMapping.DefaultLocationId,
                DefaultLocationTitle = templateMapping.DefaultLocationTitle
            };
            if (templateMapping.FieldMappings != null)
                foreach (var fieldMapping in templateMapping.FieldMappings)
                {
                    addCmsMappingModel.FieldMappings.Add(new FieldMappingModel
                    {
                        CmsTemplateId = fieldMapping.CmsField.TemplateField.FieldId,
                        GcFieldId = fieldMapping.GcField.Id,
                        GcFieldName = fieldMapping.GcField.Name
                    });
                }

            return addCmsMappingModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private static List<FieldMapping> ConvertToFieldMappings(IEnumerable<FieldMappingModel> list)
        {
            var fieldMappings = new List<FieldMapping>();
            foreach (var item in list)
            {
                var fieldMapping = new FieldMapping
                {
                    CmsField = new CmsField
                    {
                        TemplateField = new CmsTemplateField
                        {
                            FieldId = item.CmsTemplateId
                        }
                    },
                    GcField = new GcField
                    {
                        Id = item.GcFieldId,
                        Name = item.GcFieldName
                    }
                };
                fieldMappings.Add(fieldMapping);

            }
            return fieldMappings;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<MappingModel> GetMappingModel()
        {
            var mappings = FactoryRepository.Repositories.MappingRepository.GetMappings();

            var model = new List<MappingModel>();

            foreach (var templateMapping in mappings)
            {
                var mappingModel = new MappingModel
                {
                    GcProject = new GcProjectModel
                    {
                        Name = templateMapping.GcProjectName
                    },
                    GcTemplate = new GcTemplateModel
                    {
                        Id = templateMapping.GcTemplate.GcTemplateId,
                        Name = templateMapping.GcTemplate.GcTemplateName,
                    },
                    CmsTemplate = new CmsTemplateModel
                    {
                        Name = templateMapping.CmsTemplate.TemplateName,
                    },
                    MappingId = templateMapping.MappingId,
                    MappingTitle = templateMapping.MappingTitle,
                    LastMappedDateTime = templateMapping.LastMappedDateTime,
                    LastUpdatedDate = templateMapping.LastUpdatedDate,
                };
                model.Add(mappingModel);
            }

            foreach (var mapping in model)
            {
                try
                {
                    var template = GetGcTemplateEntity(mapping.GcTemplate.Id);
                    if (template == null)
                    {
                        mapping.LastUpdatedDate = "Removed from GatherContent";
                    }
                    else
                    {
                        var gcUpdateDate = ConvertMsecToDate((double)template.Data.Updated).ToLocalTime();
                        var dateFormat = FactoryService.CurrentSettings.DateFormat;
                        if (string.IsNullOrEmpty(dateFormat))
                        {
                            dateFormat = Constants.DateFormat;
                        }
                        mapping.LastUpdatedDate = gcUpdateDate.ToString(dateFormat);
                        DateTime mappedTime = DateTime.MinValue;
                        if (!string.IsNullOrWhiteSpace(mapping.LastMappedDateTime))
                        {
                            DateTime.TryParse(mapping.LastMappedDateTime, out mappedTime);
                        }
                        mapping.LastMappedDateTime = mappedTime != DateTime.MinValue ? mappedTime.ToLocalTime().ToString(dateFormat) : mapping.LastMappedDateTime;
                    }
                }
                catch (Exception)
                {
                    mapping.LastUpdatedDate = "Removed from GatherContent";
                }

            }

            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gcTemplateId"></param>
        /// <param name="cmsMappingId"></param>
        /// <returns></returns>
        public MappingModel GetSingleMappingModel(string gcTemplateId, string cmsMappingId)
        {
            if (!string.IsNullOrEmpty(gcTemplateId) && !string.IsNullOrEmpty(cmsMappingId))
            {
                var gcTemplate = GetGcTemplateEntity(gcTemplateId);
                var gcProject = GetGcProjectEntity(gcTemplate.Data.ProjectId.ToString());
                var addMappingModel = FactoryRepository.Repositories.MappingRepository.GetMappingById(cmsMappingId);
                var model = MapAddMappingModel(addMappingModel);
                model.GcProject = new GcProjectModel
                {
                    Name = gcProject.Data.Name,
                    Id = gcProject.Data.Id.ToString()
                };
                model.GcTemplate = new GcTemplateModel
                {
                    Name = gcTemplate.Data.Name,
                    Id = gcTemplate.Data.Id.ToString()
                };
                model.MappingId = cmsMappingId;
                return model;
            }

            return new MappingModel();
        }

        public List<GcProjectModel> GetGcProjectsWithMappings()
        {
            var account = GetAccount();
            var projects = GetProjects(account.Id);
            var model = new List<GcProjectModel>();

            foreach (var project in projects)
            {
                if (FactoryRepository.Repositories.MappingRepository.GetMappingsByGcProjectId(project.Id.ToString()) != null)
                    model.Add(new GcProjectModel
                    {
                        Id = project.Id.ToString(),
                        Name = project.Name
                    });
            }

            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<CmsTemplateModel> GetAvailableTemplates()
        {
            var availableTemplates = FactoryRepository.Repositories.MappingRepository.GetAvailableCmsTemplates();
            if (availableTemplates.Count == 0)
            {
                throw new Exception("Template folder is empty");
            }
            var templates = MapCmsTemplates(availableTemplates).ToList();

            return templates;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<GcProjectModel> GetAllGcProjects()
        {
            var account = GetAccount();
            var projects = GetProjects(account.Id);
            var model = new List<GcProjectModel>();

            foreach (var project in projects)
            {
                model.Add(new GcProjectModel
                {
                    Id = project.Id.ToString(),
                    Name = project.Name
                });
            }

            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gcProjectId"></param>
        /// <returns></returns>
        public List<GcTemplateModel> GetTemplatesByProjectId(string gcProjectId)
        {
            var model = new List<GcTemplateModel>();
            var templates = FactoryService.Services.TemplateService.GetTemplates(gcProjectId);
            foreach (var template in templates.Data)
            {
                model.Add(new GcTemplateModel
                {
                    Id = template.Id.ToString(),
                    Name = template.Name
                });
            }
            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gcTemplateId"></param>
        /// <returns></returns>
        public List<GcTabModel> GetFieldsByTemplateId(string gcTemplateId)
        {
            var model = new List<GcTabModel>();

            var gcTemplate = FactoryService.Services.TemplateService.GetSingleTemplate(gcTemplateId);
            foreach (var config in gcTemplate.Data.Config)
            {
                var tab = new GcTabModel { TabName = config.Label };
                foreach (var element in config.Elements)
                {
                    var tm = new GcFieldModel
                    {
                        Name = element.Label,
                        Id = element.Name,
                        Type = element.Type
                    };

                    tab.Fields.Add(tm);
                }
                model.Add(tab);
            }
            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        public void CreateMapping(MappingModel model)
        {
            var template = FactoryService.Services.TemplateService.GetSingleTemplate(model.GcTemplate.Id);
            var project = FactoryService.Services.ProjectsService.GetSingleProject(template.Data.ProjectId.ToString());

            var templateMapping = new TemplateMapping
            {
                MappingId = model.MappingId,
                MappingTitle = model.MappingTitle,
                DefaultLocationId = model.DefaultLocationId,
                LastUpdatedDate = template.Data.Updated.ToString(),
                GcProjectId = project.Data.Id.ToString(),
                GcProjectName = project.Data.Name,
                CmsTemplate = new CmsTemplate
                {
                    TemplateId = model.CmsTemplate.Id
                },
                GcTemplate = new GcTemplate
                {
                    GcTemplateId = template.Data.Id.ToString(),
                    GcTemplateName = template.Data.Name
                },
            };

            var fieldMappings = ConvertToFieldMappings(model.FieldMappings);

            templateMapping.FieldMappings = fieldMappings;
            FactoryRepository.Repositories.MappingRepository.CreateMapping(templateMapping);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        public void UpdateMapping(MappingModel model)
        {
            var template = FactoryService.Services.TemplateService.GetSingleTemplate(model.GcTemplate.Id);
            var project = FactoryService.Services.ProjectsService.GetSingleProject(template.Data.ProjectId.ToString());

            var templateMapping = new TemplateMapping
            {
                MappingId = model.MappingId,
                MappingTitle = model.MappingTitle,
                DefaultLocationId = model.DefaultLocationId,
                LastUpdatedDate = template.Data.Updated.ToString(),
                GcProjectId = project.Data.Id.ToString(),
                GcProjectName = project.Data.Name,
                CmsTemplate = new CmsTemplate
                {
                    TemplateId = model.CmsTemplate.Id
                },
                GcTemplate = new GcTemplate
                {
                    GcTemplateId = template.Data.Id.ToString(),
                    GcTemplateName = template.Data.Name
                },
            };

            var fieldMappings = ConvertToFieldMappings(model.FieldMappings);

            templateMapping.FieldMappings = fieldMappings;
            FactoryRepository.Repositories.MappingRepository.UpdateMapping(templateMapping);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scMappingId"></param>
        public void DeleteMapping(string scMappingId)
        {
            FactoryRepository.Repositories.MappingRepository.DeleteMapping(scMappingId);
        }
    }
}
