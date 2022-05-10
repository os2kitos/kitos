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
                    "$http", "$stateParams", ($http, $stateParams) => $http.get("api/itSystemUsage/" + $stateParams.id)
                        .then(result => result.data.response)
                ],
                uiState: [
                    "uiCustomizationStateService", (uiCustomizationStateService: Kitos.Services.UICustomization.IUICustomizationStateService) => uiCustomizationStateService.getCurrentState(Kitos.Models.UICustomization.CustomizableKitosModule.ItSystemUsage)
                ]
            }
        });
    }]);

    app.controller("system.UsageCtrl", ["$rootScope", "$scope", "itSystemUsage", "hasWriteAccess", "user", "",
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
            $scope.isProjectModuleEnabled = user.currentConfig.showItProjectModule && uiState.isBluePrintNodeAvailable(blueprint.children.projects);


            /**
          * <li data-ui-sref-active="active" ng-if="isFrontPageEnabled">
                 <a data-ui-sref="it-system.usage.main"><img src="/Content/img/Systemforside.svg"></img> Systemforside</a>
             </li>
             <li data-ui-sref-active="active" ng-if="isInterfacesEnabled">
                 <a data-ui-sref="it-system.usage.interfaces"><img src="/Content/img/Snitflader.svg"></img> Udstillede snitflader</a>
             </li>
             <li data-ui-sref-active="active" ng-if="isRelationsEnabled">
                 <a data-ui-sref="it-system.usage.relation"><img src="/Content/img/ITsystemer.svg"></img> Relationer</a>
             </li>
             <li data-ui-sref-active="active" ng-if="isContractsEnabled">
                 <a data-ui-sref="it-system.usage.contracts"><img src="/Content/img/Kontrakter.svg"></img> Kontrakter</a>
             </li>
             <li data-ui-sref-active="active" ng-if="isHierarchyEnabled">
                 <a data-ui-sref="it-system.usage.hierarchy"><img src="/Content/img/Hierarki.svg"></img> Hierarki</a>
             </li>
             <li data-ui-sref-active="active" ng-if="isSystemRolesEnabled">
                 <a data-ui-sref="it-system.usage.roles"><img src="/Content/img/Systemroller.svg"></img> Systemroller</a>
             </li>
             <li data-ui-sref-active="active" ng-if="isOrganizationEnabled">
                 <a data-ui-sref="it-system.usage.org"><img src="/Content/img/Organisation.svg"></img> Organisation</a>
             </li>
             <li data-ui-sref-active="active" ng-if="isLocalKleEnabled">
                 <a data-ui-sref="it-system.usage.kle"><img src="/Content/img/KLE.svg"></img> Lokale KLE</a>
             </li>
             <li data-ui-sref-active="active" ng-if="isProjectModuleEnabled">
                 <a data-ui-sref="it-system.usage.proj"><img src="/Content/img/ITprojekter.svg"></img> IT Projekter</a>
             </li>
             <li data-ui-sref-active="active" ng-if="isAdviceEnabled">
                 <a data-ui-sref="it-system.usage.advice" data-element-type="AdviceTabButton"><img src="/Content/img/Advis.svg"></img> Advis</a>
             </li>
             <li data-ui-sref-active="active" ng-if="usLocalReferencesEnabled">
                 <a data-ui-sref="it-system.usage.references"><img src="/Content/img/References.svg"></img> Lokale referencer</a>
             </li>
             <li data-ui-sref-active="active" ng-if="isArchivingEnabled">
                 <a data-ui-sref="it-system.usage.archiving"><img src="/Content/img/Arkiv.svg"></img> Arkivering</a>
             </li>
             <li data-ui-sref-active="active" ng-if="isGdprEnabled">
                 <a data-ui-sref="it-system.usage.GDPR"><img src="/Content/img/folder-lock.svg"></img> GDPR</a>
             </li>
          */
        }
    ]);
})(angular, app);
