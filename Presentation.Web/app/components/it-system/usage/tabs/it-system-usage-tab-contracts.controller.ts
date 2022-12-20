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
            var currentMainContract: number;
            
            function reloadContractState () {
                return itSystemUsageService.getItSystemUsage(usageId)
                    .then((usage) => bindContracts(usage));
            }

            function saveMainContract (id: number) {
                if (currentMainContract === id || _.isUndefined(id)) {
                    return;
                }

                apiUseCaseFactory
                    .createAssignmentCreation(() => $http.post(`api/ItContractItSystemUsage/?contractId=${id}&usageId=${usageId}`));
            };

            function deleteMainContract (id: number) {

                if (currentMainContract === id || _.isUndefined(id)) {
                    return;
                }

                apiUseCaseFactory
                    .createAssignmentRemoval(() => $http.delete(`api/ItContractItSystemUsage/?usageId=${usageId}`));
            }

            function bindContracts(usage: any) {
                $scope.contracts = usage.contracts.map(contract => {
                    return {
                        id: contract.id,
                        name: contract.name,
                        contractTypeName: contract.contractTypeName,
                        supplierName: contract.supplierName,
                        hasOperationElement: contract.hasOperationElement,
                        concluded: Kitos.Helpers.RenderFieldsHelper.renderDate(contract.concluded),
                        expirationDate: Kitos.Helpers.RenderFieldsHelper.renderDate(contract.expirationDate),
                        terminated: Kitos.Helpers.RenderFieldsHelper.renderDate(contract.terminated)
                    }
                });
                $scope.contractsToSelect = entityMapper.mapApiResponseToSelect2ViewModel(usage.contracts);

                $scope.mainContractId = usage.mainContractId;
                currentMainContract = usage.mainContractId;
                let match;
                if (usage.mainContractId !== null) {
                    match = usage.contracts && usage.contracts.find(x => { return x.id === usage.mainContractId });
                }
                itSystemUsage.mainContractIsActive = match?.isActive;
                $scope.mainContractIsActive = match?.isActive;

                $scope.mainContractViewModel = {
                    hasWriteAccess: $scope.hasWriteAccess,
                    options: entityMapper.mapApiResponseToSelect2ViewModel(usage.contracts),
                    selectedContract: match ? {id: match.id, text: match.name} : null,
                    isActive: itSystemUsage.mainContractIsActive,
                    postMethod: (id: number) => saveMainContract(id),
                    deleteMethod: (id: number) => deleteMainContract(id),
                    stateReloadMethod: () => reloadContractState(),
                }
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
