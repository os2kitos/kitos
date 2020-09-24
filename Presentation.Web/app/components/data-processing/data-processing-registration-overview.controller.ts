module Kitos.DataProcessing.Registration.Overview {
    "use strict";

    export interface IOverviewController extends Utility.KendoGrid.IGridViewAccess<Models.DataProcessing.IDataProcessingRegistration> {
    }

    export class OverviewController implements IOverviewController {
        mainGrid: IKendoGrid<Models.DataProcessing.IDataProcessingRegistration>;
        mainGridOptions: IKendoGridOptions<Models.DataProcessing.IDataProcessingRegistration>;
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
            $rootScope.page.title = "Databehandling - Overblik";

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
                    .create<Models.DataProcessing.IDataProcessingRegistration>()
                    .withScope($scope)
                    .withGridBinding(this)
                    .withUser(user)
                    .withEntityTypeName("Databehandling")
                    .withExcelOutputName("Databehandling - Overblik")
                    .withStorageKey("data-processing-registration-overview-options")
                    .withFixedSourceUrl(`/odata/Organizations(${user.currentOrganizationId})/DataProcessingRegistrationReadModels?$expand=RoleAssignments`)
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
                    .withSchemaFields({
                        AgreementConcludedAt: { type: "date" }
                    })
                    .withToolbarEntry({
                        id: "createDpa",
                        title: "Opret Registrering",
                        color: Utility.KendoGrid.KendoToolbarButtonColor.Green,
                        position: Utility.KendoGrid.KendoToolbarButtonPosition.Right,
                        enabled: () => userAccessRights.canCreate,
                        onClick: () => $state.go("data-processing.overview.create-registration")
                    } as Utility.KendoGrid.IKendoToolbarEntry)
                    .withColumn(builder =>
                        builder
                            .withDataSourceName("Name")
                            .withTitle("Databehandling")
                            .withId("dpaName")
                            .withStandardWidth(200)
                            .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                            .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference("kendo-dpa-name-rendering", "data-processing.edit-registration.main", dataItem.SourceEntityId, dataItem.Name))
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
                    .withColumn(builder =>
                        builder
                            .withDataSourceName("DataProcessorNamesAsCsv")
                            .withTitle("Databehandlere")
                            .withId("dpDataProcessorNamesAsCsv")
                            .withStandardWidth(150)
                            .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                            .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(dataItem.DataProcessorNamesAsCsv))
                            .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(dataItem.DataProcessorNamesAsCsv)))
                    .withColumn(builder =>
                        builder
                            .withDataSourceName("IsAgreementConcluded")
                            .withTitle("Databehandleraftale er indgået")
                            .withId("agreementConcluded")
                            .withStandardWidth(100)
                            .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                            .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(dataItem.IsAgreementConcluded))
                            .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(dataItem.IsAgreementConcluded)))
                    .withColumn(builder =>
                        builder
                            .withDataSourceName("AgreementConcludedAt")
                            .withTitle("Dato for indgåelse af databehandleraftale")
                            .withId("agreementConcludedAt")
                            .withStandardWidth(150)
                            .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Date)
                            .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.AgreementConcludedAt))
                            .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderDate(dataItem.AgreementConcludedAt))
                            .withInitialVisibility(false))
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
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference(`kendo-dpa-${getRoleKey(role)}-rendering`, "data-processing.edit-registration.roles", dataItem.SourceEntityId, dpaRoleIdToUserNamesMap[dataItem.Id][role.Id]))
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
                    templateUrl: "app/components/data-processing/data-processing-registration-overview.view.html",
                    controller: OverviewController,
                    controllerAs: "vm",
                    resolve: {
                        user: [
                            "userService", userService => userService.getUser()
                        ],
                        roles: [
                            "localOptionServiceFactory", (localOptionServiceFactory: Services.LocalOptions.ILocalOptionServiceFactory) =>
                                localOptionServiceFactory.create(Services.LocalOptions.LocalOptionType.DataProcessingRegistrationRoles).getAll()
                        ],
                        userAccessRights: ["authorizationServiceFactory", (authorizationServiceFactory: Services.Authorization.IAuthorizationServiceFactory) =>
                            authorizationServiceFactory
                                .createDataProcessingRegistrationAuthorization()
                                .getOverviewAuthorization()]
                    }
                });
            }
        ]);
}
