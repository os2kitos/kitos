(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system-usage.proj', {
            url: '/proj',
            templateUrl: 'partials/it-system/tab-proj.html',
            controller: 'system.EditProj',
        });
    }]);

    app.controller('system.EditProj', ['$scope', function ($scope) {
        
    }]);
})(angular, app);