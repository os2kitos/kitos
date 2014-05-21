(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.plan', {
            url: '/plan',
            templateUrl: 'partials/it-contract/it-contract-plan.html',
            controller: 'contract.PlanCtrl',
            resolve: {

            }
        });
    }]);

    app.controller('contract.PlanCtrl', ['$scope', '$http', 'notify',
            function ($scope, $http, notify) {

            }]);
})(angular, app);