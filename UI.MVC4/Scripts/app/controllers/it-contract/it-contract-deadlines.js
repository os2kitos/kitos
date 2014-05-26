(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.edit.deadlines', {
            url: '/agreement-periods',
            templateUrl: 'partials/it-contract/tab-deadlines.html',
            controller: 'contract.DeadlinesCtrl',
            resolve: {

            }
        });
    }]);

    app.controller('contract.DeadlinesCtrl', ['$scope', '$http', 'notify', 'contract',
            function ($scope, $http, notify, contract) {
                $scope.contract = contract;
            }]);
})(angular, app);