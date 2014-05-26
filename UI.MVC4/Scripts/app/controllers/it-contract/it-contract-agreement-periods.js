(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.edit.agreement-periods', {
            url: '/agreement-periods',
            templateUrl: 'partials/it-contract/tab-agreement-periods.html',
            controller: 'contract.AgreementPeriodsCtrl',
            resolve: {

            }
        });
    }]);

    app.controller('contract.AgreementPeriodsCtrl', ['$scope', '$http', 'notify', 'contract',
            function ($scope, $http, notify, contract) {

            }]);
})(angular, app);