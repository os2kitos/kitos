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

            //Prepare the page
            $rootScope.page.title = "Databehandleraftaler - Overblik";

            //Helper functions
            const getRoleKey = (role: Models.IOptionEntity) => `role${role.Id}`;

            const replaceRoleQuery = (filterUrl, roleName, roleId) => {
                var pattern = new RegExp(`(\\w+\\()${roleName}(,.*?\\))`, "i");
                return filterUrl.replace(pattern, `RoleAssignments/any(c: $1c/UserFullName$2 and c/RoleId eq ${roleId})`);
            };

            //Lookup maps
            var dpaRoleIdToUserNamesMap = {};

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

                        if (parameterMap.$filter) {
                            roles.forEach(role => {
                                parameterMap.$filter =
                                    replaceRoleQuery(parameterMap.$filter, getRoleKey(role), role.Id);
                            });
                        }

                        return parameterMap;
                    })
                    .withResponseParser(response => {
                        //Reset all response state
                        dpaRoleIdToUserNamesMap = {};

                        //Build lookups/mutations
                        response.forEach(dpa => {
                            dpaRoleIdToUserNamesMap[dpa.Id] = {};

                            //Update the role assignment map
                            dpa.RoleAssignments.forEach(assignment => {
                                if (!dpaRoleIdToUserNamesMap[dpa.Id][assignment.RoleId])
                                    dpaRoleIdToUserNamesMap[dpa.Id][assignment.RoleId] = assignment.UserFullName;
                                else {
                                    dpaRoleIdToUserNamesMap[dpa.Id][assignment.RoleId] += `, ${assignment.UserFullName}`;
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
                            .withStandardWidth(200)
                            .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                            .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference("kendo-dpa-name-rendering", "data-processing.edit-agreement.main", dataItem.SourceEntityId, dataItem.Name))
                            .withSourceValueEchoExcelOutput())
                    .withColumn(builder =>
                        builder
                            .withDataSourceName("MainReferenceTitle")
                            .withTitle("Reference")
                            .withId("dpReferenceId")
                            .withStandardWidth(150)
                            .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                            .withRendering(dataItem => Helpers.RenderFieldsHelper.renderReference(dataItem.MainReferenceTitle, dataItem.MainReferenceUrl))
                            .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderReference(dataItem.MainReferenceTitle, dataItem.MainReferenceUrl)))
                    .withColumn(builder =>
                        builder
                            .withDataSourceName("MainReferenceUserAssignedId")
                            .withTitle("Dokument ID / Sagsnr.")
                            .withId("dpReferenceUserAssignedId")
                            .withStandardWidth(150)
                            .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                            .withRendering(dataItem => Helpers.RenderFieldsHelper.renderReferenceId(dataItem.MainReferenceUserAssignedId))
                            .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderReferenceId(dataItem.MainReferenceUserAssignedId))
                            .withInitialVisibility(false))
                    .withColumn(builder =>
                        builder
                            .withDataSourceName("SystemNamesAsCsv")
                            .withTitle("IT Systemer")
                            .withId("dpSystemNamesAsCsv")
                            .withStandardWidth(150)
                            .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                            .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(dataItem.SystemNamesAsCsv))
                            .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(dataItem.SystemNamesAsCsv)))
                    .withStandardSorting("Name");

            roles.forEach(role =>
                launcher = launcher.withColumn(builder =>
                    builder
                        .withDataSourceName(getRoleKey(role))
                        .withTitle(role.Name)
                        .withId(`dpa${getRoleKey(role)}`)
                        .withStandardWidth(150)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withoutSorting() //Sorting is not possible on expressions which are required on role columns since they are generated in the UI as a result of content of a complex typed child collection
                        .withInitialVisibility(false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference(`kendo-dpa-${getRoleKey(role)}-rendering`, "data-processing.edit-agreement.roles", dataItem.SourceEntityId, dpaRoleIdToUserNamesMap[dataItem.Id][role.Id]))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(dpaRoleIdToUserNamesMap[dataItem.Id][role.Id])))
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
