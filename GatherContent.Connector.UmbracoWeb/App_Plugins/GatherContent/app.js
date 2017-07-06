angular.module('umbraco').run(['$location',
     function ($location) {
         if ($location.path() == '/GatherContent/')
        $location.path('/GatherContent/GatherContentTree/edit/settings');
    }]
);
