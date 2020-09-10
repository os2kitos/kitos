module Kitos.DataProcessing.Agreement.Overview {
    "use strict";

    export interface IOverviewController extends Utility.KendoGrid.IGridViewAccess<Models.DataProcessing.IDataProcessingAgreement> {
    }

    export class OverviewController implements IOverviewController {
        mainGrid: IKendoGrid<Models.DataProcessing.IDataProcessingAgreement>;
        mainGridOptions: IKendoGridOptions<Models.DataProcessing.IDataProcessingAgreement>;
        canCreate: boolean;
        projectIdToAccessLookup = {};

        static $inject: Array<string> = [
            "$rootScope",
            "$scope",
            "$state",
            "user",
            "userAccessRights",
            "kendoGridLauncherFactory",
            "roles"
        ];

        constructor(
            $rootScope: IRootScope,
            $scope: ng.IScope,
            $state: ng.ui.IStateService,
            user,
            userAccessRights: Models.Api.Authorization.EntitiesAccessRightsDTO,
            kendoGridLauncherFactory: Utility.KendoGrid.IKendoGridLauncherFactory,
            roles: Models.IOptionEntity[]) {
            $rootScope.page.title = "Databehandleraftaler - Overblik";

            var replaceRoleQuery = (filterUrl, roleName, roleId) => {
                var pattern = new RegExp(`(\\w+\\()${roleName}(,.*?\\))`, "i");
                var newurl = filterUrl.replace(pattern, `RoleAssignments/any(c: $1c/UserFullName$2 and c/RoleId eq ${roleId})`);
                return newurl;
            }

            var dpaToRoleMap = {};

            //Build and launch kendo grid
            var launcher =
                kendoGridLauncherFactory
                    .create<Models.DataProcessing.IDataProcessingAgreement>()
                    .withScope($scope)
                    .withGridBinding(this)
                    .withUser(user)
                    .withEntityTypeName("Databehandleraftale")
                    .withExcelOutputName("Databehandleraftaler - Overblik")
                    .withStorageKey("data-processing-agreement-overview-options")
                    .withFixedSourceUrl(`/odata/Organizations(${user.currentOrganizationId})/DataProcessingAgreementReadModels?$expand=RoleAssignments`)
                    .withParameterMapping((options, type) => {
                        // get kendo to map parameters to an odata url
                        var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);

                        //TODO: Allow sorting on roles - replace with "any" for that
                        if (parameterMap.$filter) {
                            roles.forEach(role => {
                                parameterMap.$filter =
                                    replaceRoleQuery(parameterMap.$filter, `role${role.Id}`, role.Id);
                            });
                        }

                        return parameterMap;
                    })
                    .withResponseParser(response => {
                        //Reset all response state
                        dpaToRoleMap = {}; 

                        //Build lookups/mutations
                        response.forEach(dpa => {
                            dpaToRoleMap[dpa.Id] = {};

                            //Update the role assignment map
                            dpa.RoleAssignments.forEach(assignment => {
                                if (!dpaToRoleMap[dpa.Id][assignment.RoleId])
                                    dpaToRoleMap[dpa.Id][assignment.RoleId] = assignment.UserFullName;
                                else {
                                    dpaToRoleMap[dpa.Id][assignment.RoleId] += `, ${assignment.UserFullName}`;
                                }
                            });
                        });
                        return response;
                    })
                    .withToolbarEntry({
                        id: "createDpa",
                        title: "Opret Databehandleraftale",
                        color: Utility.KendoGrid.KendoToolbarButtonColor.Green,
                        position: Utility.KendoGrid.KendoToolbarButtonPosition.Right,
                        enabled: () => userAccessRights.canCreate,
                        onClick: () => $state.go("data-processing.overview.create-agreement")
                    } as Utility.KendoGrid.IKendoToolbarEntry)
                    .withColumn(builder =>
                        builder
                            .withDataSourceName("Name")
                            .withTitle("Databehandleraftale")
                            .withId("dpaName")
                            .withStandardWidth(350)
                            .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                            .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference("kendo-dpa-name-rendering", "data-processing.overview.edit-agreement.main", dataItem.SourceEntityId, dataItem.Name))
                            .withSourceValueEchoExcelOutput())
                    .withStandardSorting("Name");

            roles.forEach(role =>
                launcher = launcher.withColumn(builder =>
                    builder
                        .withDataSourceName(`role${role.Id}`)
                        .withTitle(role.Name)
                        .withId(`dpaRole${role.Id}`)
                        .withStandardWidth(135)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference(`kendo-dpa-role-${role.Id}-rendering`, "data-processing.edit-agreement.roles", dataItem.SourceEntityId, dpaToRoleMap[dataItem.Id][role.Id]))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(dpaToRoleMap[dataItem.Id][role.Id])))
            );

            //Launch kendo grid
            launcher.launch();
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
                        roles: [
                            "localOptionServiceFactory", (localOptionServiceFactory: Services.LocalOptions.ILocalOptionServiceFactory) =>
                                localOptionServiceFactory.create(Services.LocalOptions.LocalOptionType.DataProcessingAgreementRoles).getAll()
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
