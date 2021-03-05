(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.usage.contracts', {
            url: '/contracts',
            templateUrl: 'app/components/it-system/usage/tabs/it-system-usage-tab-contracts.view.html',
            controller: 'system.EditContracts',
            resolve: {
            }
        });
    }]);

    app.controller('system.EditContracts', ['$scope', '$http', '$state', '$stateParams', '$timeout', 'itSystemUsage', 'notify',
        function ($scope, $http, $state, $stateParams, $timeout, itSystemUsage, notify) {
            var usageId = itSystemUsage.id;

            $scope.usage = itSystemUsage;
            $scope.contracts = itSystemUsage.contracts;
            $scope.mainContractId = itSystemUsage.mainContractId;

            function reload() {
                $state.go('.', null, { reload: true });
            };

            $scope.saveMainContract = function () {
                var msg = notify.addInfoMessage("Gemmer... ");
                if ($scope.mainContractId) {
                    $http.post('api/ItContractItSystemUsage/?contractId=' + $scope.mainContractId + '&usageId=' + usageId)
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("Gemt!");
                            reload();
                        }, function onError(result) {
                            msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                        });
                } else {
                    $http.delete('api/ItContractItSystemUsage/?usageId=' + usageId)
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("Gemt!");
                            reload();
                        }, function onError(result) {
                            msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                        });
                }
            }
        }]);
})(angular, app);
