(function () {
    'use strict';

    function UmbLoadIndicatorDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: '/App_Plugins/GatherContent/directives/umb-load-indicator.html'
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbLoadIndicator', UmbLoadIndicatorDirective);

})();