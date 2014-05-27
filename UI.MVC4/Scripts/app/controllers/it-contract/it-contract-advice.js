(function(ng, app) {
    app.config(['$stateProvider', function($stateProvider) {
        $stateProvider.state('it-contract.edit.advice', {
            url: '/advice',
            templateUrl: 'partials/it-contract/tab-advice.html',
            controller: 'contract.EditAdviceCtrl',
            resolve: {
                itContractRoles: ['$http', function ($http) {
                    return $http.get("api/itcontractrole/")
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('contract.EditAdviceCtrl', ['$scope', '$http', 'notify', 'contract', 'itContractRoles',
        function ($scope, $http, notify, contract, itContractRights, itContractRoles) {


        }]);

})(angular, app);