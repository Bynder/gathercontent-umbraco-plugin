using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GatherContent.Connector.Entities.Entities;
using GatherContent.Connector.IRepositories.Interfaces;
using GatherContent.Connector.IRepositories.Models.Import;
using GatherContent.Connector.IRepositories.Models.Mapping;
using GatherContent.Connector.UmbracoRepositories.DataBaseModels;
using GatherContent.Connector.UmbracoRepositories.Helpers;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;

namespace GatherContent.Connector.UmbracoRepositories.Repositories
{
    public class MappingRepository : BaseRepository, IMappingRepository
    {
        public List<TemplateMapping> GetMappings()
        {
            List<TemplateMapping> resultMaps = new List<TemplateMapping>();

            var mappings = ContextDatabase.Database.Fetch<UmbProject>(new Sql().Select("*").From<UmbProject>());
            foreach (var umbMapping in mappings)
            {
                var template =
                    ContextDatabase.Database.Fetch<UmbTemplateMapping>(
                        new Sql().Select("*").From<UmbTemplateMapping>().Where<UmbTemplateMapping>(x => x.ProjectId == umbMapping.Id));
                resultMaps.AddRange(template.Select(umbTemplate => CreateTemplateMappingObject(umbMapping, umbTemplate)));
            }

            return resultMaps;
        }

        public List<TemplateMapping> GetMappingsByGcTemplateId(string gcTemplateId)
        {
            int templId;
            if (!string.IsNullOrWhiteSpace(gcTemplateId) && int.TryParse(gcTemplateId, out templId))
            {
                var templates =
                    ContextDatabase.Database.Fetch<UmbTemplateMapping>(
                        new Sql().Select("*")
                            .From<UmbTemplateMapping>()
                            .Where<UmbTemplateMapping>(x => x.GcTemplateId == templId));

                if (templates != null)
                    return new List<TemplateMapping>(templates.Select(umbTemplate => new
                    {
                        umbTemplate,
                        mapping = ContextDatabase.Database.Fetch<UmbProject>(
                            new Sql().Select("*")
                                .From<UmbProject>()
                                .Where<UmbProject>(x => x.Id == umbTemplate.ProjectId)).FirstOrDefault()
                    }).Select(@t => CreateTemplateMappingObject(@t.mapping, @t.umbTemplate)));
            }
            return null;
        }

        public List<TemplateMapping> GetMappingsByGcProjectId(string projectId)
        {
            int projectIdInt;
            if (!string.IsNullOrWhiteSpace(projectId) && int.TryParse(projectId, out projectIdInt))
            {
                var mappings =
                    ContextDatabase.Database.Fetch<UmbProject>(
                        new Sql().Select("*")
                            .From<UmbProject>()
                            .Where<UmbProject>(x => x.GcProjectId == projectIdInt));
                if (mappings != null && mappings.Count > 0)
                {
                    var result = new List<TemplateMapping>();
                    foreach (var mapping in mappings)
                    {
                        var templates =
                        ContextDatabase.Database.Fetch<UmbTemplateMapping>(
                            new Sql().Select("*").From<UmbTemplateMapping>().Where<UmbTemplateMapping>(x => x.ProjectId == mapping.Id));
                        result.AddRange(
                            templates.Select(umbTemplate => CreateTemplateMappingObject(mapping, umbTemplate)));
                    }
                    return result;
                }
            }
            return null;
        }

        public TemplateMapping GetMappingById(string id)
        {
            int intId = 0;
            if (!string.IsNullOrWhiteSpace(id) && int.TryParse(id, out intId))
            {
                var template =
                    ContextDatabase.Database.First<UmbTemplateMapping>(
                        new Sql().Select("*").From<UmbTemplateMapping>().Where<UmbTemplateMapping>(x => x.Id == intId));
                if (template != null)
                {
                    var mapping =
                        ContextDatabase.Database.First<UmbProject>(
                            new Sql().Select("*").From<UmbProject>().Where<UmbProject>(x => x.Id == template.ProjectId));
                    if (mapping != null)
                        return CreateTemplateMappingObject(mapping, template);
                }
            }

            return null;
        }

        public List<CmsTemplate> GetAvailableCmsTemplates()
        {
            return ContextService.ContentTypeService.GetAllContentTypes().Select(x => new CmsTemplate()
            {
                TemplateId = x.Id.ToString(),
                TemplateName = x.Name,
                TemplateFields = GetCmsTypes(x).ToList()
            }).ToList();
        }

        public void CreateMapping(TemplateMapping cmsMapping)
        {
            var mapping = new UmbProject()
            {
                GcProjectId = Convert.ToInt32(cmsMapping.GcProjectId),
                GcProjectName = cmsMapping.GcProjectName,
                CreatedDate = DateTime.UtcNow.ToIsoString()
            };
            if (cmsMapping.MappingId != null)
                mapping =
                    ContextDatabase.Database.Fetch<UmbProject>(
                        new Sql().Select("*")
                            .From<UmbProject>()
                            .Where<UmbProject>(x => x.Id.ToString() == cmsMapping.MappingId)).FirstOrDefault() ?? mapping;
                    
            mapping.Id = Convert.ToInt32(ContextDatabase.Database.Insert("gcProject", "Id", mapping));
            var gcProjectTemp = new UmbTemplateMapping()
            {
                GcTemplateName = cmsMapping.GcTemplate.GcTemplateName,
                GcTemplateId = Convert.ToInt32(cmsMapping.GcTemplate.GcTemplateId),
                LastUpdatedDate = cmsMapping.LastUpdatedDate,
                LastMappedDateTime = DateTime.UtcNow.ToIsoString(),
                CreatedDate = DateTime.UtcNow.ToIsoString(),
                ProjectId = mapping.Id,
                CmsTemplateId = cmsMapping.CmsTemplate != null ? Convert.ToInt32(cmsMapping.CmsTemplate.TemplateId) : 0,
                CmsTemplateName = (cmsMapping.CmsTemplate?.TemplateId != null)
                    ? ContextService.ContentTypeService.GetContentType(Convert.ToInt32(cmsMapping.CmsTemplate.TemplateId)).Name : null,
                MappingTitle = cmsMapping.MappingTitle,
                DefaultLocation = cmsMapping.DefaultLocationId
            };
            gcProjectTemp.Id = Convert.ToInt32(ContextDatabase.Database.Insert("gcTemplateMapping", "Id", gcProjectTemp));
            foreach (var fieldMapping in cmsMapping.FieldMappings)
            {
                var umbFieldMapping = new UmbFieldMapping()
                {
                    CmsTemplateFieldId = fieldMapping.CmsField.TemplateField.FieldId,
                    CmsTemplateFieldName = fieldMapping.CmsField.TemplateField.FieldName,
                    CmsTemplateFieldType = fieldMapping.CmsField.TemplateField.FieldType,
                    GcFieldId = fieldMapping.GcField.Id,
                    GcFieldName = fieldMapping.GcField.Name,
                    GcFieldType = fieldMapping.GcField.Type,
                    TemplateMappingId = gcProjectTemp.Id
                };
                ContextDatabase.Database.Insert("gcFieldMapping", "Id", umbFieldMapping);
            }
        }

        public void UpdateMapping(TemplateMapping cmsMapping)
        {
            int mappingId = Convert.ToInt32(cmsMapping.MappingId);
            var gcProjectTemp =
                ContextDatabase.Database.First<UmbTemplateMapping>(
                    new Sql().Select("*")
                        .From<UmbTemplateMapping>()
                        .Where<UmbTemplateMapping>(x => x.Id == mappingId));
            if (gcProjectTemp != null)
            {
                gcProjectTemp.MappingTitle = cmsMapping.MappingTitle;
                gcProjectTemp.GcTemplateName = cmsMapping.GcTemplate.GcTemplateName;
                gcProjectTemp.GcTemplateId = Convert.ToInt32(cmsMapping.GcTemplate.GcTemplateId);
                gcProjectTemp.LastUpdatedDate = cmsMapping.LastUpdatedDate;
                gcProjectTemp.LastMappedDateTime = DateTime.UtcNow.ToIsoString();
                gcProjectTemp.UpdatedDate = DateTime.UtcNow.ToIsoString();
                gcProjectTemp.CmsTemplateId = Convert.ToInt32(cmsMapping.CmsTemplate.TemplateId);
                gcProjectTemp.CmsTemplateName = (cmsMapping.CmsTemplate?.TemplateId != null)
                        ? ContextService.ContentTypeService.GetContentType(Convert.ToInt32(cmsMapping.CmsTemplate.TemplateId)).Name : null;
                gcProjectTemp.DefaultLocation = cmsMapping.DefaultLocationId;


                ContextDatabase.Database.Update("gcTemplateMapping", "Id", gcProjectTemp);

                var project =
                    ContextDatabase.Database.First<UmbProject>(
                        new Sql().Select("*")
                            .From<UmbProject>()
                            .Where<UmbProject>(x => x.Id == gcProjectTemp.ProjectId));

                if (project == null)
                    return;
                project.GcProjectId = Convert.ToInt32(cmsMapping.GcProjectId);
                project.GcProjectName = cmsMapping.GcProjectName;
                project.UpdatedDate = DateTime.UtcNow.ToIsoString();

                ContextDatabase.Database.Update("gcProject", "Id", project);

                ////////////Deleting old fields
                DeleteFiledSMappingsByTepmlateId(gcProjectTemp.Id);
                ///////////Creating new fields
                foreach (var fieldMapping in cmsMapping.FieldMappings)
                {
                    var umbFieldMapping = new UmbFieldMapping()
                    {
                        CmsTemplateFieldId = fieldMapping.CmsField.TemplateField.FieldId,
                        CmsTemplateFieldName = fieldMapping.CmsField.TemplateField.FieldName,
                        CmsTemplateFieldType = fieldMapping.CmsField.TemplateField.FieldType,
                        GcFieldId = fieldMapping.GcField.Id,
                        GcFieldName = fieldMapping.GcField.Name,
                        GcFieldType = fieldMapping.GcField.Type,
                        TemplateMappingId = gcProjectTemp.Id
                    };
                    ContextDatabase.Database.Insert("gcFieldMapping", "Id", umbFieldMapping);
                }
            }
        }

        public void DeleteMapping(string id)
        {
            int intId;
            if (!string.IsNullOrWhiteSpace(id) && int.TryParse(id, out intId))
            {
                var template =
                    ContextDatabase.Database.Single<UmbTemplateMapping>(
                        new Sql().Select("*").From<UmbTemplateMapping>().Where<UmbTemplateMapping>(x => x.Id == intId));

                //deleting filed mapping
                DeleteFiledSMappingsByTepmlateId(template.Id);

                ContextDatabase.Database.Delete<UmbTemplateMapping>(template);

                var count = ContextDatabase.Database.Fetch<UmbTemplateMapping>(
                    new Sql().Select("*").From<UmbTemplateMapping>().Where<UmbTemplateMapping>(x => x.ProjectId == template.ProjectId))
                    .Count;

                if (count == 0)
                {
                    ContextDatabase.Database.Delete<UmbProject>(template.ProjectId);
                }
            }
            return;
        }

        private void DeleteFiledSMappingsByTepmlateId(int templateId)
        {
            ContextDatabase.Database.Fetch<UmbFieldMapping>(
                    new Sql().Select("*")
                        .From<UmbFieldMapping>()
                        .Where<UmbFieldMapping>(x => x.TemplateMappingId == templateId))
                    .ForEach(fieldMapping => ContextDatabase.Database.Delete<UmbFieldMapping>(fieldMapping));
        }

        private TemplateMapping CreateTemplateMappingObject(UmbProject mapping, UmbTemplateMapping template)
        {
            var fieldMappings =
                        ContextDatabase.Database.Fetch<UmbFieldMapping>(
                            new Sql().Select("*")
                                .From<UmbFieldMapping>()
                                .Where<UmbFieldMapping>(x => x.TemplateMappingId == template.Id));



            return new TemplateMapping()
            {
                MappingId = template.Id.ToString(),                           //MappingId == umbraco's templateId
                GcProjectId = mapping.GcProjectId.ToString(),
                GcProjectName = mapping.GcProjectName,
                LastUpdatedDate = template.LastUpdatedDate,
                DefaultLocationId = template.DefaultLocation,
                DefaultLocationTitle = !string.IsNullOrWhiteSpace(template.DefaultLocation) ?
                    ContextService.ContentService.GetById(Convert.ToInt32(template.DefaultLocation))?.Name : string.Empty,
                CmsTemplate =
                    new CmsTemplate()
                    {
                        TemplateId = template.CmsTemplateId.ToString(),
                        TemplateName = template.CmsTemplateName
                    },
                GcTemplate =
                    new GcTemplate()
                    {
                        GcTemplateId = template.GcTemplateId.ToString(),
                        GcTemplateName = template.GcTemplateName
                    },
                LastMappedDateTime = template.LastMappedDateTime,
                MappingTitle = template.MappingTitle,
                FieldMappings = fieldMappings.Count > 0 ? new List<FieldMapping>(fieldMappings.Select(x => new FieldMapping()
                {
                    GcField =
                        new GcField() { Id = x.GcFieldId, Name = x.GcFieldName, Type = x.GcFieldType },
                    CmsField =
                        new CmsField()
                        {
                            TemplateField =
                                new CmsTemplateField()
                                {
                                    FieldName = x.CmsTemplateFieldName,
                                    FieldId = x.CmsTemplateFieldId,
                                    FieldType = x.CmsTemplateFieldType
                                }
                        }
                })) : new List<FieldMapping>()
            };
        }

        private IList<CmsTemplateField> GetCmsTypes(IContentType contentType)
        {
            var result = new List<CmsTemplateField>();
            result.AddRange(
                    contentType.CompositionPropertyTypes.Select(
                        type =>
                            new CmsTemplateField()
                            {
                                FieldId = type.Id.ToString(),
                                FieldName = type.Name,
                                FieldType =
                                    ContextService.DataTypeService.GetDataTypeDefinitionById(
                                        type.DataTypeDefinitionId).PropertyEditorAlias
                            }));
            return result;
        }
    }
}
