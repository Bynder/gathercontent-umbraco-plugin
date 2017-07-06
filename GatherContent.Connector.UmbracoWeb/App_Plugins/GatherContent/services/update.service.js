(function() {
    'use strict';
    angular.module('umbraco.resources').factory('updateResource', function($http) {
        return {
            getItems: function (id) {
                return $http.get('backoffice/GatherContent/Update/Get?id=' + id);
            },
            updateItems: function(id, items) {
                return $http.post('backoffice/GatherContent/Update/UpdateItems?id=' + id, items);
            }
        }
    });
})()