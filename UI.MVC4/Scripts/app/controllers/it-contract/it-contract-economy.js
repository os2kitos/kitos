(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.edit.economy', {
            url: '/economy',
            templateUrl: 'partials/it-contract/tab-economy.html',
            controller: 'contract.EditEconomyCtrl',
            resolve: {
            }
        });
    }]);

    app.controller('contract.EditEconomyCtrl', ['$scope', '$http', 'notify', 'contract',
        function ($scope, $http, notify, contract) {


        }]);

})(angular, app);