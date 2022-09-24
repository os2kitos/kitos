((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage", {
            url: "/usage/{id:[0-9]+}",
            templateUrl: "app/components/it-system/usage/it-system-usage.view.html",
            controller: "system.UsageCtrl",
            resolve: {
                user: [
                    "userService", userService => userService.getUser()
                ],
                userAccessRights: ["authorizationServiceFactory", "$stateParams",
                    (authorizationServiceFactory: Kitos.Services.Authorization.IAuthorizationServiceFactory, $stateParams) =>
                        authorizationServiceFactory
                            .createSystemUsageAuthorization()
                            .getAuthorizationForItem($stateParams.id)
                ],
                hasWriteAccess: [
                    "userAccessRights", (userAccessRights: Kitos.Models.Api.Authorization.EntityAccessRightsDTO) => userAccessRights.canEdit
                ],
                itSystemUsage: [
                    "itSystemUsageService", "$stateParams", (itSystemUsageService: Kitos.Services.ItSystemUsage.IItSystemUsageService, $stateParams) => itSystemUsageService.getItSystemUsage($stateParams.id)
                ],
                uiState: [
                    "uiCustomizationStateService", (uiCustomizationStateService: Kitos.Services.UICustomization.IUICustomizationStateService) => uiCustomizationStateService.getCurrentState(Kitos.Models.UICustomization.CustomizableKitosModule.ItSystemUsage)
                ],
                contractUiState: [
                    "uiCustomizationStateService", (uiCustomizationStateService: Kitos.Services.UICustomization.IUICustomizationStateService) => uiCustomizationStateService.getCurrentState(Kitos.Models.UICustomization.CustomizableKitosModule.ItContract)
                ]
            }
        });
    }]);

    app.controller("system.UsageCtrl", ["$rootScope", "$scope", "itSystemUsage", "hasWriteAccess", "user", "uiState",
        ($rootScope, $scope, itSystemUsage, hasWriteAccess, user, uiState: Kitos.Models.UICustomization.ICustomizedModuleUI) => {
            $scope.hasWriteAccess = hasWriteAccess;
            $scope.usage = itSystemUsage;

            $scope.usageViewModel = new Kitos.Models.ViewModel.ItSystemUsage.SystemUsageViewModel(itSystemUsage);
            $scope.systemUsageName = Kitos.Helpers.SystemNameFormat.apply(`${itSystemUsage.itSystem.name} - i ${itSystemUsage.organization.name}`, itSystemUsage.itSystem.disabled);

            $scope.allowClearOption = {
                allowClear: true
            };

            if (!$scope.hasWriteAccess) {
                _.remove($rootScope.page.subnav.buttons, (o: any) => o.text === "Fjern anvendelse");
            }

            // Setup available tabs
            const blueprint = Kitos.Models.UICustomization.Configs.BluePrints.ItSystemUsageUiCustomizationBluePrint;
            $scope.isFrontPageEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.frontPage);
            $scope.isInterfacesEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.interfaces);
            $scope.isRelationsEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.systemRelations);
            $scope.isDataProcessingEnabled = user.currentConfig.showDataProcessing && uiState.isBluePrintNodeAvailable(blueprint.children.dataProcessing);
            $scope.isContractsEnabled = user.currentConfig.showItContractModule && uiState.isBluePrintNodeAvailable(blueprint.children.contracts);
            $scope.isHierarchyEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.hierarchy);
            $scope.isSystemRolesEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.systemRoles);
            $scope.isOrganizationEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.organization);
            $scope.isLocalKleEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.localKle);
            $scope.isAdviceEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.advice);
            $scope.isLocalReferencesEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.localReferences);
            $scope.isArchivingEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.archiving);
            $scope.isGdprEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.gdpr);
        }
    ]);
})(angular, app);
