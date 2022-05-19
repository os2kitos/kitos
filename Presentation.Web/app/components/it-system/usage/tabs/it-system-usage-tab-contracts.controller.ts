((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.contracts", {
            url: "/contracts",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-contracts.view.html",
            controller: "system.EditContracts"
        });
    }]);

    app.controller("system.EditContracts", ["$scope", "$http", "itSystemUsage", "notify", "entityMapper", "uiState",
        ($scope, $http, itSystemUsage, notify, entityMapper, uiState: Kitos.Models.UICustomization.ICustomizedModuleUI) => {
            var usageId = itSystemUsage.id;

            $scope.usage = itSystemUsage;
            $scope.contracts = entityMapper.mapApiResponseToSelect2ViewModel(itSystemUsage.contracts);
            $scope.mainContractId = itSystemUsage.mainContractId;

            $scope.saveMainContract = id => {
                if (itSystemUsage.mainContractId === id || _.isUndefined(id)) {
                    return;
                }
                var msg = notify.addInfoMessage("Gemmer... ");
                if (id) {
                    $http.post("api/ItContractItSystemUsage/?contractId=" + id + "&usageId=" + usageId)
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("Gemt!");
                            var contracts = itSystemUsage.contracts;
                            var match = contracts && contracts.find(x => { return x.id === id });
                            itSystemUsage.mainContractIsActive = match && match.isActive;
                        },
                            function onError(result) {
                                msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                            });
                    itSystemUsage.mainContractId = id;
                } else {
                    $http.delete("api/ItContractItSystemUsage/?usageId=" + usageId)
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("Gemt!");
                            itSystemUsage.mainContractIsActive = false;
                        },
                            function onError(result) {
                                msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                            });
                    itSystemUsage.mainContractId = null;
                }
            };

            //UI Customization
            const blueprint = Kitos.Models.UICustomization.Configs.BluePrints.ItSystemUsageUiCustomizationBluePrint;
            $scope.showMainContractSelection = uiState.isBluePrintNodeAvailable(blueprint.children.contracts.children.selectContractToDetermineIfItSystemIsActive);
        }]);
})(angular, app);
