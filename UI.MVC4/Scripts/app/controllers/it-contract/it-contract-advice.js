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
        function ($scope, $http, notify, contract, itContractRoles) {
            console.log(itContractRoles);
            $scope.itContractRoles = itContractRoles;

            var baseUrl = "api/advice";
            
            var advices = [];
            $scope.advices = advices;
            
            function pushAdvice(advice) {
                advice.show = true;
                advice.updateUrl = baseUrl + "/" + advice.id;

                advice.delete = function () {
                    var msg = notify.addInfoMessage("Sletter rækken...", false);
                    
                    $http.delete(advice.updateUrl).success(function(result) {
                        advice.show = false;
                        msg.toSuccessMessage("Rækken er slettet!");
                    }).error(function () {
                        msg.toErrorMessage("Fejl! Rækken kunne ikke slettes!");
                    });
                };

                advices.push(advice);
            }

            _.each(contract.advices, pushAdvice);

            function newAdvice() {

                var data = {
                    itContractId: contract.id,
                    isActive: true
                };

                var msg = notify.addInfoMessage("Tilføjer ny række...", false);

                $http.post(baseUrl, data).success(function(result) {
                    pushAdvice(result.response);
                    msg.toSuccessMessage("Rækken er tilføjet!");
                }).error(function() {
                    msg.toErrorMessage("Fejl! Kunne ikke tilføje række!");
                });
            }

            $scope.newAdvice = newAdvice;

        }]);

})(angular, app);