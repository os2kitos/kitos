(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('edit-it-project.org', {
            url: '/org',
            templateUrl: 'partials/it-project/tab-org.html',
            controller: 'project.EditOrg',
            resolve: {
                
            }
        });
    }]);

    app.controller('project.EditOrg', ['$scope', '$http', '$stateParams', function ($scope, $http, $stateParams) {
        
    }]);
})(angular, app);