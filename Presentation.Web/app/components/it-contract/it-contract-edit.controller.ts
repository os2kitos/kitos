((ng, app) => {
    app.config([
        "$stateProvider", $stateProvider => {
            $stateProvider.state("it-contract.edit", {
                url: "/edit/{id:[0-9]+}",
                templateUrl: "app/components/it-contract/it-contract-edit.view.html",
                controller: "contract.EditCtrl",
                controllerAs: "contractEditVm",
                resolve: {
                    contract: ["$http", "$stateParams", ($http, $stateParams) => $http.get("api/itcontract/" + $stateParams.id).then(result => result.data.response)
                    ],
                    user: ["userService", userService => userService.getUser().then(user => user)
                    ],
                    userAccessRights: ["authorizationServiceFactory", "$stateParams",
                        (authorizationServiceFactory : Kitos.Services.Authorization.IAuthorizationServiceFactory, $stateParams) =>
                            authorizationServiceFactory
                                .createContractAuthorization()
                                .getAuthorizationForItem($stateParams.id)
                    ],
                    hasWriteAccess: ["userAccessRights", (userAccessRights: Kitos.Models.Api.Authorization.EntityAccessRightsDTO) => userAccessRights.canEdit],
                    uiState: [
                        "uiCustomizationStateService", (uiCustomizationStateService: Kitos.Services.UICustomization.IUICustomizationStateService) => uiCustomizationStateService.getCurrentState(Kitos.Models.UICustomization.CustomizableKitosModule.ItContract)
                    ],
                    criticalityOptions: [
                        "localOptionServiceFactory",
                        (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                        localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.CriticalityTypes)
                        .getAll()
                    ],
                }
            });
        }
    ]);

    app.controller("contract.EditCtrl", ["$scope", "$rootScope", "contract", "hasWriteAccess", "userAccessRights", "user", "uiState",
        ($scope, $rootScope, contract, hasWriteAccess, userAccessRights: Kitos.Models.Api.Authorization.EntityAccessRightsDTO, user, uiState: Kitos.Models.UICustomization.ICustomizedModuleUI) => {
            $scope.hasWriteAccess = hasWriteAccess;
            $scope.allowClearOption = {
                allowClear: true
            };
            $scope.contract = contract;

            if (!userAccessRights.canDelete) {
                _.remove($rootScope.page.subnav.buttons, (o: any) => o.text === "Slet IT Kontrakt");
            }

            const blueprint = Kitos.Models.UICustomization.Configs.BluePrints.ItContractUiCustomizationBluePrint;

            $scope.isFrontPageEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.frontPage);
            $scope.isItSystemsEnabled = user.currentConfig.showItSystemModule && uiState.isBluePrintNodeAvailable(blueprint.children.itSystems);
            $scope.isDataProcessingEnabled = user.currentConfig.showDataProcessing && uiState.isBluePrintNodeAvailable(blueprint.children.dataProcessing);
            $scope.isDeadlinesEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.deadlines);
            $scope.isEconomyEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.economy);
            $scope.isContractRolesEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.contractRoles);
            $scope.isHierarchyEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.hierarchy);
            $scope.isAdviceEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.advice);
            $scope.isReferencesEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.references);
        }]);
})(angular, app);
