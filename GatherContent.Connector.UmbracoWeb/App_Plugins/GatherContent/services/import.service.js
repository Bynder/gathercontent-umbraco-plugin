(function () {
    'use strict';

    angular.module('umbraco.resources').factory('importResource', function ($http) {
        return {
            get: function (projectId, templateId) {
                if (!!templateId)
                    return $http.get('backoffice/GatherContent/Import/GetItems?projectId=' + projectId + '&templateId=' + templateId);
                else
                    if (!!projectId)
                        return $http.get('backoffice/GatherContent/Import/GetItems?projectId=' + projectId);
                    else 
                        return $http.get('backoffice/GatherContent/Import/GetItems');
            },
            getStatuses: function(projectId) {
                return $http.get('backoffice/GatherContent/Import/GetFilters?projectId=' + projectId);
            },
            getProjectsWithMapp: function () {
                return $http.get('backoffice/GatherContent/Import/GetProjectsWithMapp');
            },
            importItems: function (items, projectId, statusId, id) {
                return $http.post('backoffice/GatherContent/Import/Post?projectId=' + projectId + '&statusId=' + statusId + '&id=' + id, items);
            },
            importItemsLocations: function (items, projectId, statusId) {
                return $http.post('backoffice/GatherContent/Import/PostWithLocations?projectId=' + projectId + '&statusId=' + statusId, items);
            }
        }
    });
})()