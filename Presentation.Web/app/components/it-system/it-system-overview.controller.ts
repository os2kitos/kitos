module Kitos.ItSystem.Overview {
    "use strict";

    export interface IOverviewController {
        mainGrid: IKendoGrid<IItSystemUsageOverview>;
        mainGridOptions: kendo.ui.GridOptions;
    }

    export interface IItSystemUsageOverview extends Models.ItSystemUsage.IItSystemUsageOverviewReadModel {
        roles: Array<string>;
    }

    export class OverviewController implements IOverviewController {
        private storageKey = "it-system-overview-options";

        mainGrid: IKendoGrid<IItSystemUsageOverview>;
        mainGridOptions: IKendoGridOptions<IItSystemUsageOverview>;
        static $inject: Array<string> = [
            "$rootScope",
            "$scope",
            "$window",
            "systemRoles",
            "user",
            "orgUnits",
            "kendoGridLauncherFactory"
        ];

        constructor(
            $rootScope: IRootScope,
            $scope: any,
            private readonly $window: ng.IWindowService,
            private readonly systemRoles: Array<Models.IOptionEntity>,
            private readonly user,
            private readonly orgUnits: Array<any>,
            kendoGridLauncherFactory: Utility.KendoGrid.IKendoGridLauncherFactory) {
            $rootScope.page.title = "IT System - Overblik";

            //Helper functions
            const getRoleKey = (role: Models.IOptionEntity) => `role${role.Id}`;

            const replaceRoleQuery = (filterUrl, roleName, roleId) => {
                var pattern = new RegExp(`(\\w+\\()${roleName}(,.*?\\))`, "i");
                return filterUrl.replace(pattern, `RoleAssignments/any(c: $1c/UserFullName$2 and c/RoleId eq ${roleId})`);
            };

            //Lookup maps
            var roleIdToUserNamesMap = {};

            //Build and launch kendo grid
            var launcher = kendoGridLauncherFactory
                .create<Models.ItSystemUsage.IItSystemUsageOverviewReadModel>()
                .withScope($scope)
                .withGridBinding(this)
                .withUser(user)
                .withEntityTypeName("IT System")
                .withExcelOutputName("IT Systeme Overblik")
                .withStorageKey(this.storageKey)
                .withFixedSourceUrl(
                    `/odata/Organizations(${user.currentOrganizationId
                    })/ItSystemUsageOverviewReadModels`)
                .withParameterMapping((options, type) => {
                    // get kendo to map parameters to an odata url
                    var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);
                    if (parameterMap.$filter) {
                        systemRoles.forEach(role => {
                            parameterMap.$filter =
                                replaceRoleQuery(parameterMap.$filter, getRoleKey(role), role.Id);
                        });
                    }
                    return parameterMap;
                })
                .withResponseParser(response => {
                    //Reset all response state
                    roleIdToUserNamesMap = {};

                    //Build lookups/mutations
                    response.forEach(systemUsage => {
                        roleIdToUserNamesMap[systemUsage.Id] = {};

                        //Update the role assignment map
                        if (systemUsage.RoleAssignments) {
                            systemUsage.RoleAssignments.forEach(assignment => {
                                if (!roleIdToUserNamesMap[systemUsage.Id][assignment.RoleId])
                                    roleIdToUserNamesMap[systemUsage.Id][assignment.RoleId] = assignment.UserFullName;
                                else {
                                    roleIdToUserNamesMap[systemUsage.Id][assignment.RoleId] += `, ${assignment.UserFullName}`;
                                }
                            });
                        }
                    });
                    return response;
                })
                .withToolbarEntry({
                    id: "gdprExportAnchor",
                    title: "Exportér GPDR data til Excel",
                    color: Utility.KendoGrid.KendoToolbarButtonColor.Grey,
                    position: Utility.KendoGrid.KendoToolbarButtonPosition.Right,
                    implementation: Utility.KendoGrid.KentoToolbarImplementation.Link,
                    enabled: () => true,
                    link: `api/v1/gdpr-report/csv/${this.user.currentOrganizationId}`
                } as Utility.KendoGrid.IKendoToolbarEntry)
                .withColumn(builder =>
                    builder
                        .withDataSourceName("IsActive")
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Boolean)
                        .withTitle("Gyldig/Ikke gyldig")
                        .withId("isActive")
                        .withStandardWidth(90)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange([
                            {
                                textValue: "Gyldig",
                                remoteValue: true
                            },
                            {
                                textValue: "Ikke gyldig",
                                remoteValue: false
                            }
                        ],
                            false)
                        .withRendering(dataItem => dataItem.IsActive ? '<span class="fa fa-file text-success" aria-hidden="true"></span>' : '<span class="fa fa-file-o text-muted" aria-hidden="true"></span>')
                        .withExcelOutput(dataItem => dataItem.IsActive ? "Ja" : "Nej"))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("LocalSystemId")
                        .withTitle("Lokal System ID")
                        .withId("localid")
                        .withStandardWidth(150)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput()
                        .withInitialVisibility(false))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ItSystemUuid")
                        .withTitle("UUID")
                        .withId("uuid")
                        .withStandardWidth(150)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput()
                        .withInitialVisibility(false))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ParentItSystemName")
                        .withTitle("Overordnet IT System")
                        .withId("parentsysname")
                        .withStandardWidth(150)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference(`kendo-parent-system-rendering`, "it-system.edit.main", dataItem.ParentItSystemId, Helpers.SystemNameFormat.apply(dataItem.ParentItSystemName, false))) //TODO: Missing property from backend
                        .withSourceValueEchoExcelOutput()
                        .withInitialVisibility(false))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("Name")
                        .withTitle("IT System")
                        .withId("sysname")
                        .withStandardWidth(320)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference(`kendo-system-usage-rendering`, "it-system.usage.main", dataItem.SourceEntityId, Helpers.SystemNameFormat.apply(dataItem.Name, dataItem.ItSystemDisabled)))
                        .withExcelOutput(dataItem => dataItem.Name))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("Version")
                        .withTitle("Version")
                        .withId("version")
                        .withStandardWidth(150)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput()
                        .withInitialVisibility(false))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("LocalCallName")
                        .withTitle("Lokal kaldenavn")
                        .withId("localname")
                        .withStandardWidth(150)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput()
                        .withInitialVisibility(false))
                .withStandardSorting("Name");

            this.systemRoles.forEach(role =>
                launcher = launcher.withColumn(builder =>
                    builder
                        .withDataSourceName(getRoleKey(role))
                        .withTitle(role.Name)
                        .withId(`systemUsage${getRoleKey(role)}`)
                        .withStandardWidth(145)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withoutSorting() //Sorting is not possible on expressions which are required on role columns since they are generated in the UI as a result of content of a complex typed child collection
                        .withInitialVisibility(false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference(`kendo-system-usage-${getRoleKey(role)}-rendering`, "it-system.usage.roles", dataItem.SourceEntityId, roleIdToUserNamesMap[dataItem.Id][role.Id]))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(roleIdToUserNamesMap[dataItem.Id][role.Id])))
            );

            //Launch kendo grid
            launcher.launch();
        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", $stateProvider => {
                $stateProvider.state("it-system.overview", {
                    url: "/overview",
                    templateUrl: "app/components/it-system/it-system-overview.view.html",
                    controller: OverviewController,
                    controllerAs: "systemOverviewVm",
                    resolve: {
                        systemRoles: [
                            "localOptionServiceFactory", (localOptionServiceFactory: Services.LocalOptions.ILocalOptionServiceFactory) =>
                                localOptionServiceFactory.create(Services.LocalOptions.LocalOptionType.ItSystemRoles).getAll()
                        ],
                        user: [
                            "userService", userService => userService.getUser()
                        ],
                        orgUnits: [
                            "$http", "user", "_",
                            ($http, user, _) => $http.get(`/odata/Organizations(${user.currentOrganizationId})/OrganizationUnits`)
                                .then(result => _.addHierarchyLevelOnFlatAndSort(result.data.value, "Id", "ParentId"))
                        ]
                    }
                });
            }
        ]);
}
