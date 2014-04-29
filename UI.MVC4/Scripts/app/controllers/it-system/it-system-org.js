(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system-usage.org', {
            url: '/org',
            templateUrl: 'partials/it-system/tab-org.html',
            controller: 'system.EditOrg',
        });
    }]);

    app.controller('system.EditOrg', ['$scope', function ($scope) {
        
    }]);
})(angular, app);