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

    app.controller('system.EditContracts', ['$scope', '$http', '$state', '$stateParams', '$timeout', 'itSystemUsage', 'notify', 'entityMapper',
        function ($scope, $http, $state, $stateParams, $timeout, itSystemUsage, notify, entityMapper) {
            var usageId = itSystemUsage.id;

            $scope.usage = itSystemUsage;
            $scope.contracts = entityMapper.mapApiResponseToSelect2ViewModel(itSystemUsage.contracts);
            $scope.mainContractId = itSystemUsage.mainContractId;


            $scope.saveMainContract = function (id) {
                if (itSystemUsage.mainContractId === id || _.isUndefined(id)) {
                    return;
                }
                var msg = notify.addInfoMessage("Gemmer... ");
                if (id) {
                    $http.post('api/ItContractItSystemUsage/?contractId=' + id + '&usageId=' + usageId)
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("Gemt!");
                        }, function onError(result) {
                            msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                        });
                    itSystemUsage.mainContractId = id;
                } else {
                    $http.delete('api/ItContractItSystemUsage/?usageId=' + usageId)
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("Gemt!");
                        }, function onError(result) {
                            msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                        });
                    itSystemUsage.mainContractId = null;
                }
            }
        }]);
})(angular, app);
