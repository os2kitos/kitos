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
        ($scope, $http, itSystemUsage,
            entityMapper: Kitos.Services.LocalOptions.IEntityMapper,
            uiState: Kitos.Models.UICustomization.ICustomizedModuleUI,
            apiUseCaseFactory: Kitos.Services.Generic.IApiUseCaseFactory,
            contractUiState: Kitos.Models.UICustomization.ICustomizedModuleUI,
            itSystemUsageService: Kitos.Services.ItSystemUsage.IItSystemUsageService) => {
            var usageId = itSystemUsage.id;
            bindContracts(itSystemUsage);
            
            function reloadContractState () {
                return itSystemUsageService.getItSystemUsage(usageId)
                    .then((usage) => bindContracts(usage));
            }

            function saveMainContract(id: number): ng.IPromise<void> {

                return apiUseCaseFactory
                    .createAssignmentCreation<void>(() => $http.post(`api/ItContractItSystemUsage/?contractId=${id}&usageId=${usageId}`))
                    .executeAsync();
            };

            function deleteMainContract (): ng.IPromise<void> {

                return apiUseCaseFactory
                    .createAssignmentRemoval<void>(() => $http.delete(`api/ItContractItSystemUsage/?usageId=${usageId}`))
                    .executeAsync();
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
                 
                let match;
                if (usage.mainContractId !== null) {
                    match = usage.contracts && usage.contracts.find(x => { return x.id === usage.mainContractId });
                }
                itSystemUsage.mainContractIsActive = match?.isActive;
                $scope.mainContractIsActive = match?.isActive;

                $scope.mainContractViewModel = {
                    options: entityMapper.mapApiResponseToSelect2ViewModel(usage.contracts),
                    selectedContractId: match ? match.id : null,
                    isActive: itSystemUsage.mainContractIsActive,
                    postMethod: (id: number) => saveMainContract(id),
                    deleteMethod: () => deleteMainContract(),
                    stateReloadMethod: () => reloadContractState(),
                } as Kitos.Shared.Components.IMainContractSectionViewModel;
            }

            //UI Customization
            const blueprint = Kitos.Models.UICustomization.Configs.BluePrints.ItSystemUsageUiCustomizationBluePrint;
            const contractBlueprint = Kitos.Models.UICustomization.Configs.BluePrints.ItContractUiCustomizationBluePrint;

            $scope.showMainContractSelection = uiState.isBluePrintNodeAvailable(blueprint.children.contracts.children.selectContractToDetermineIfItSystemIsActive);
            $scope.showContractIsActive = contractUiState.isBluePrintNodeAvailable(contractBlueprint.children.frontPage.children.isActive);
            $scope.showMainContract = $scope.showMainContractSelection && $scope.showContractIsActive;

            $scope.showContractType = contractUiState.isBluePrintNodeAvailable(contractBlueprint.children.frontPage.children.contractType);
            $scope.showContractAgreementPeriod = contractUiState.isBluePrintNodeAvailable(contractBlueprint.children.frontPage.children.agreementPeriod);
            $scope.showContractAgreementTermination = contractUiState.isBluePrintNodeAvailable(contractBlueprint.children.deadlines.children.termination);
        }]);
})(angular, app);
