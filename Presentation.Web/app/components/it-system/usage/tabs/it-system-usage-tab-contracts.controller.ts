((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.contracts", {
            url: "/contracts",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-contracts.view.html",
            controller: "system.EditContracts"
        });
    }]);

    app.controller("system.EditContracts", ["$scope", "$http", "itSystemUsage", "entityMapper", "uiState", "apiUseCaseFactory", "contractUiState",
        ($scope, $http, itSystemUsage, entityMapper, uiState: Kitos.Models.UICustomization.ICustomizedModuleUI, apiUseCaseFactory: Kitos.Services.Generic.IApiUseCaseFactory, contractUiState: Kitos.Models.UICustomization.ICustomizedModuleUI) => {
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
            const contractBlueprint = Kitos.Models.UICustomization.Configs.BluePrints.ItContractUiCustomizationBluePrint;

            $scope.showMainContractSelection = uiState.isBluePrintNodeAvailable(blueprint.children.contracts.children.selectContractToDetermineIfItSystemIsActive);

            $scope.showContractIsActive = contractUiState.isBluePrintNodeAvailable(contractBlueprint.children.frontPage.children.isActive);
            $scope.showContractType = contractUiState.isBluePrintNodeAvailable(contractBlueprint.children.frontPage.children.contractType);
            $scope.showContractAgreementPeriod = contractUiState.isBluePrintNodeAvailable(contractBlueprint.children.frontPage.children.agreementPeriod);
            $scope.showContractAgreementTermination = contractUiState.isBluePrintNodeAvailable(contractBlueprint.children.deadlines.children.termination);
        }]);
})(angular, app);
