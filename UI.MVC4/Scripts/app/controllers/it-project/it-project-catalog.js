(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.catalog', {
            url: '/catalog',
            templateUrl: 'partials/it-project/catalog.html',
            controller: 'project.EditCatalogCtrl'
        });
    }]);

    app.controller('project.EditCatalogCtrl',
        ['$scope', function($scope) {

        }]);
})(angular, app);