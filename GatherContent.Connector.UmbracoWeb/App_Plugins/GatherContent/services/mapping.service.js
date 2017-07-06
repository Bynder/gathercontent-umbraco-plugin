(function () {
	'use strict';

	angular.module('umbraco.resources').factory('mappingResource', function ($http) {
		return {
			getAll: function () {
				return $http.get('backoffice/GatherContent/Mapping/GetAll');
			},
			getSingle: function (gcId, cmsId) {
			    return $http.get('backoffice/GatherContent/Mapping/Get?gcId=' + gcId + '&cmsId=' + cmsId);
			},
			getAllProjects: function () {
			    return $http.get('backoffice/GatherContent/Mapping/GetAllProjects');
			},
            getUmbTemplates: function() {
                return $http.get('backoffice/GatherContent/Mapping/GetAvailableTemplates');
            },
			getTemplates: function (id) {
			    return $http.get('backoffice/GatherContent/Mapping/GetTemplatesByProject?id=' + id);
			},
			getFieldsByTemplate: function (id) {
			    return $http.get('backoffice/GatherContent/Mapping/GetFieldsByTemplateId?id=' + id);
			},
			saveMapping: function (mapping) {
			    return $http.post('backoffice/GatherContent/Mapping/Post', JSON.stringify(mapping));
			},
		    deleteMap: function(id) {
			    return $http.delete("backoffice/GatherContent/Mapping/DeleteMapping?temlateId=" + id);
		    }
		}
	});
})()