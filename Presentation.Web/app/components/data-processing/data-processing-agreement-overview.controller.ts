module Kitos.DataProcessing.Agreement.Overview {
    "use strict";

    export interface IOverviewController extends Utility.KendoGrid.IGridViewAccess<Models.DataProcessing.IDataProcessingAgreement> {
    }

    export class OverviewController implements IOverviewController {
        public mainGrid: IKendoGrid<Models.DataProcessing.IDataProcessingAgreement>;
        public mainGridOptions: IKendoGridOptions<Models.DataProcessing.IDataProcessingAgreement>;
        public canCreate: boolean;
        public projectIdToAccessLookup = {};

        public static $inject: Array<string> = [
            "$rootScope",
            "$scope",
            "$state",
            "user",
            "userAccessRights",
            "kendoGridLauncherFactory"
        ];

        constructor(
            $rootScope: IRootScope,
            $scope: ng.IScope,
            $state: ng.ui.IStateService,
            user,
            userAccessRights: Models.Api.Authorization.EntitiesAccessRightsDTO,
            kendoGridLauncherFactory: Utility.KendoGrid.IKendoGridLauncherFactory) {
            $rootScope.page.title = "Databehandleraftaler - Overblik";

            kendoGridLauncherFactory
                .create<Models.DataProcessing.IDataProcessingAgreement>()
                .withScope($scope)
                .withUser(user)
                .withEntityTypeName("Databehandleraftale")
                .withExcelOutputName("Databehandleraftaler - Overblik")
                .withStorageKey("data-processing-agreement-overview-options")
                .withGridBinding(this)
                .withFixedSourceUrl(`/odata/Organizations(${user.currentOrganizationId})/DataProcessingAgreementReadModels`)
                .withToolbarEntry({
                    id: "createDpa",
                    title: "Opret Databehandleraftale",
                    color: Utility.KendoGrid.KendoToolbarButtonColor.Green,
                    position: Utility.KendoGrid.KendoToolbarButtonPosition.Right,
                    enabled: () => userAccessRights.canCreate,
                    onClick: () => $state.go("data-processing.overview.create-agreement")
                } as Utility.KendoGrid.IKendoToolbarEntry)
                .launch();
        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", $stateProvider => {
                $stateProvider.state("data-processing.overview", {
                    url: "/overview",
                    templateUrl: "app/components/data-processing/data-processing-agreement-overview.view.html",
                    controller: OverviewController,
                    controllerAs: "vm",
                    resolve: { 
                        user: [
                            "userService", userService => userService.getUser()
                        ],
                        userAccessRights: ["authorizationServiceFactory", (authorizationServiceFactory: Services.Authorization.IAuthorizationServiceFactory) =>
                            authorizationServiceFactory 
                                .createDataProcessingAgreementAuthorization()
                                .getOverviewAuthorization()]
                    }
                });
            }
        ]);
}
