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

    app.controller("system.EditContracts", ["$scope", "$http", "itSystemUsage", "entityMapper", "uiState", "contractUiState", "itSystemUsageService",
        ($scope, $http, itSystemUsage,
            entityMapper: Kitos.Services.LocalOptions.IEntityMapper,
            uiState: Kitos.Models.UICustomization.ICustomizedModuleUI,
            contractUiState: Kitos.Models.UICustomization.ICustomizedModuleUI,
            itSystemUsageService: Kitos.Services.ItSystemUsage.IItSystemUsageService) => {
            var usageId = itSystemUsage.id;
            bindContracts(itSystemUsage);

            function reloadContractState() {
                return itSystemUsageService.getItSystemUsage(usageId)
                    .then((usage) => bindContracts(usage));
            }

            function saveMainContract(id: number): ng.IPromise<void> {
                return $http.post(`api/ItContractItSystemUsage/?contractId=${id}&usageId=${usageId}`)
            };

            function deleteMainContract(): ng.IPromise<void> {
                return $http.delete(`api/ItContractItSystemUsage/?usageId=${usageId}`);
            }

            function bindContracts(usage: any) {
                $scope.contracts = usage
                    .contracts
                    .sort((a, b) => a.name.localeCompare(b.name, Kitos.Shared.Localization.danishLocale))
                    .map(contract => {
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
                    selectContract: (id: number) => saveMainContract(id),
                    deselectContract: () => deleteMainContract(),
                    reloadSelectedContractState: () => reloadContractState(),
                } as Kitos.Shared.Components.IMainContractSelectionViewModel;
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
