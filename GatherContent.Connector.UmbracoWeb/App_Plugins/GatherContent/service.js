angular.module("umbraco.resources")
 .factory("gatherResource", function ($http) {
     return {
         getProjectList: function() {
             return $http.get("backoffice/GatherContent/GCProject/GetProjects"); 
         },
         getTemplatesList: function (projectId) {
             return $http.get("backoffice/GatherContent/GCProject/GetTemplates?projectId=" + projectId);
         },
         getGcTemplateTabs: function(id) {
             return $http.get("backoffice/GatherContent/GatherContentMappings/GetGcTemplateTabs?templateId=" + id);
         },

         getUTemlates: function() {
             return $http.get("backoffice/GatherContent/UmbracoDocTypes/GetDocTypes");
         }
     };
 });