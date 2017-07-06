(function() {
    'use strict';

    angular.module('umbraco.resources').factory('accountResource', function ($http) {
        return {
            getAll: function() {
                return $http.get('backoffice/GatherContent/AccountSetting/Get');
            },
            save: function (settings) {
            	return $http.post('backoffice/GatherContent/AccountSetting/Post', angular.toJson(settings));
            },
            testSetting: function() {
                return $http.get('backoffice/GatherContent/AccountSetting/TestSettings');
            }
        }
    });
})()