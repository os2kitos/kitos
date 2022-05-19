((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.contracts", {
            url: "/contracts",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-contracts.view.html",
            controller: "system.EditContracts"
        });
    }]);

    app.controller("system.EditContracts", ["$scope", "$http", "itSystemUsage", "entityMapper", "uiState", "apiUseCaseFactory",
        ($scope, $http, itSystemUsage, entityMapper, uiState: Kitos.Models.UICustomization.ICustomizedModuleUI, apiUseCaseFactory: Kitos.Services.Generic.IApiUseCaseFactory) => {
            var usageId = itSystemUsage.id;

            $scope.usage = itSystemUsage;
            $scope.contracts = entityMapper.mapApiResponseToSelect2ViewModel(itSystemUsage.contracts);
            $scope.mainContractId = itSystemUsage.mainContractId;

            $scope.saveMainContract = id => {
                if (itSystemUsage.mainContractId === id || _.isUndefined(id)) {
                    return;
                }
                if (id) {
                    apiUseCaseFactory
                        .createAssignmentCreation(() => $http.post(`api/ItContractItSystemUsage/?contractId=${id}&usageId=${usageId}`))
                        .executeAsync((_) => {
                            const contracts = itSystemUsage.contracts;
                            const match = contracts && contracts.find(x => { return x.id === id });
                            itSystemUsage.mainContractIsActive = match && match.isActive;
                        }
                        );
                    itSystemUsage.mainContractId = id;
                } else {
                    apiUseCaseFactory
                        .createAssignmentRemoval(() => $http.delete(`api/ItContractItSystemUsage/?usageId=${usageId}`))
                        .executeAsync((_) => itSystemUsage.mainContractIsActive = false);
                    itSystemUsage.mainContractId = null;
                }
            };

            //UI Customization
            const blueprint = Kitos.Models.UICustomization.Configs.BluePrints.ItSystemUsageUiCustomizationBluePrint;
            $scope.showMainContractSelection = uiState.isBluePrintNodeAvailable(blueprint.children.contracts.children.selectContractToDetermineIfItSystemIsActive);
        }]);
})(angular, app);
