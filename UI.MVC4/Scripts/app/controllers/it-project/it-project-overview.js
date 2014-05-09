(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.overview', {
            url: '/overview',
            templateUrl: 'partials/it-project/overview.html',
            controller: 'project.EditOverviewCtrl'
        });
    }]);

    app.controller('project.EditOverviewCtrl',
    ['$scope',
        function ($scope) {
            
        }]);
})(angular, app);
