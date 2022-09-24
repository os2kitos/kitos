((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.contracts", {
            url: "/contracts",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-contracts.view.html",
            controller: "system.EditContracts",
            resolve: {
                itSystemUsage: [
                    "itSystemUsageService", "$stateParams", (itSystemUsageService: Kitos.Services.ItSystemUsage.IItSystemUsageService, $stateParams) => itSystemUsageService.getItSystemUsage($stateParams.id)
                ]
            }
        });
    }]);

    app.controller("system.EditContracts", ["$scope", "$http", "itSystemUsage", "entityMapper", "uiState", "apiUseCaseFactory", "contractUiState", "itSystemUsageService",
        ($scope, $http, itSystemUsage, entityMapper, uiState: Kitos.Models.UICustomization.ICustomizedModuleUI, apiUseCaseFactory: Kitos.Services.Generic.IApiUseCaseFactory, contractUiState: Kitos.Models.UICustomization.ICustomizedModuleUI, itSystemUsageService: Kitos.Services.ItSystemUsage.IItSystemUsageService) => {
            var usageId = itSystemUsage.id;
            bindContracts(itSystemUsage);
            var currentMainContract = null;

            const reloadContractState = () => {
                return itSystemUsageService.getItSystemUsage(usageId)
                    .then((usage) => bindContracts(usage));
            }

            $scope.saveMainContract = id => {
                if (currentMainContract === id || _.isUndefined(id)) {
                    return;
                }
                if (id) {
                    apiUseCaseFactory
                        .createAssignmentCreation(() => $http.post(`api/ItContractItSystemUsage/?contractId=${id}&usageId=${usageId}`))
                        .executeAsync((_) => reloadContractState());
                } else {
                    apiUseCaseFactory
                        .createAssignmentRemoval(() => $http.delete(`api/ItContractItSystemUsage/?usageId=${usageId}`))
                        .executeAsync((_) => reloadContractState());
                }
            };

            function bindContracts(usage) {
                $scope.usage = usage;
                $scope.contracts = entityMapper.mapApiResponseToSelect2ViewModel(usage.contracts);
                $scope.mainContractId = usage.mainContractId;
                currentMainContract = usage.mainContractId;
                let match
                if (usage.mainContractId !== null) {
                    match = usage.contracts && usage.contracts.find(x => { return x.id === usage.mainContractId });
                }
                itSystemUsage.mainContractIsActive = match?.isActive;

            }

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
