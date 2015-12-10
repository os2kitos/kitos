(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.usage.contracts', {
            url: '/contracts',
            templateUrl: 'partials/it-system/tab-contracts.html',
            controller: 'system.EditContracts',
            resolve: {
            }
        });
    }]);

    app.controller('system.EditContracts', ['$scope', '$http', '$state', '$stateParams', '$timeout', 'itSystemUsage', 'notify',
        function ($scope, $http, $state, $stateParams, $timeout, itSystemUsage, notify) {
            var usageId = itSystemUsage.id;
            $scope.contracts = itSystemUsage.contracts;
            $scope.mainContractId = itSystemUsage.mainContractId;

            function reload() {
                $state.go('.', null, { reload: true });
            };

            $scope.saveMainContract = function () {
                var msg = notify.addInfoMessage("Gemmer... ");
                if ($scope.mainContractId) {
                    $http.post('api/ItContractItSystemUsage/?contractId=' + $scope.mainContractId + '&usageId=' + usageId)
                        .success(function () {
                            msg.toSuccessMessage("Gemt!");
                            reload();
                        })
                        .error(function () {
                            msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                        });
                } else {
                    $http.delete('api/ItContractItSystemUsage/?usageId=' + usageId)
                        .success(function () {
                            msg.toSuccessMessage("Gemt!");
                            reload();
                        })
                        .error(function () {
                            msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                        });
                }
            }
        }]);
})(angular, app);
