(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.catalog', {
            url: '/catalog',
            templateUrl: 'partials/it-contract/it-contract-catalog.html',
            controller: 'contract.CatalogCtrl',
            resolve: {

            }
        });
    }]);

    app.controller('contract.CatalogCtrl', ['$scope', '$http', 'notify',
            function ($scope, $http, notify) {

            }]);
})(angular, app);