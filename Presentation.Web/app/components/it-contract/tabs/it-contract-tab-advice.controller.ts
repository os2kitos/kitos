(function(ng, app) {
    app.config(['$stateProvider', function($stateProvider) {
        $stateProvider.state('it-contract.edit.advice', {
            url: '/advice',
            templateUrl: 'app/components/it-contract/tabs/it-contract-tab-advice.view.html',
            controller: 'contract.EditAdviceCtrl',
            resolve: {
                itContractRoles: ['$http', function ($http) {
                    return $http.get("odata/LocalItContractRoles?$filter=IsActive eq true or IsObligatory eq true")
                        .then(function (result) {
                            return result.data.value;
                        });
                }],
                advices: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get('api/itcontract/' + $stateParams.id).then(function (result) {
                        return result.data.response.advices;
                    });
                }]
            }
        });
    }]);

    app.controller('contract.EditAdviceCtrl', ['$scope', '$http', '$state', '$stateParams', '$timeout', 'notify', 'contract', 'advices', 'itContractRoles', 'user',
        function ($scope, $http, $state, $stateParams, $timeout, notify, contract, advices, itContractRoles, user) {
            $scope.itContractRoles = itContractRoles;
            $scope.advices = advices;

            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            var baseUrl = "api/advice";

            _.each(advices, pushAdvice);

            function pushAdvice(advice) {
                advice.updateUrl = baseUrl + "/" + advice.id;

                advice.delete = function () {
                    var msg = notify.addInfoMessage("Sletter rækken...", false);

                    $http.delete(advice.updateUrl + '?organizationId=' + user.currentOrganizationId).success(function (result) {
                        msg.toSuccessMessage("Rækken er slettet!");
                        reload();
                    }).error(function () {
                        msg.toErrorMessage("Fejl! Rækken kunne ikke slettes!");
                    });
                };
            }

            $scope.newAdvice = function() {
                var data = {
                    itContractId: contract.id,
                    isActive: true
                };

                var msg = notify.addInfoMessage("Tilføjer ny række...", false);

                $http.post(baseUrl, data).success(function(result) {
                    pushAdvice(result.response);
                    msg.toSuccessMessage("Rækken er tilføjet!");
                    reload();
                }).error(function() {
                    msg.toErrorMessage("Fejl! Kunne ikke tilføje række!");
                });
            };

            // work around for $state.reload() not updating scope
            // https://github.com/angular-ui/ui-router/issues/582
            function reload() {
                return $state.transitionTo($state.current, $stateParams, {
                    reload: true
                }).then(function () {
                    $scope.hideContent = true;
                    return $timeout(function () {
                        return $scope.hideContent = false;
                    }, 1);
                });
            };
        }]);
})(angular, app);
