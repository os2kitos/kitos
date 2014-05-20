(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.overview', {
            url: '/overview',
            templateUrl: 'partials/it-contract/overview.html',
            controller: 'contract.OverviewCtrl',
            resolve: {

            }
        });
    }]);

    app.controller('contract.OverviewCtrl', ['$scope', '$http', 'notify',
            function ($scope, $http, notify) {

            }]);
})(angular, app);