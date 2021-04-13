module Kitos.ItSystem.Overview {
    "use strict";

    export interface IOverviewController {
        mainGrid: Kitos.IKendoGrid<IItSystemUsageOverview>;
        mainGridOptions: kendo.ui.GridOptions;
    }

    export interface IItSystemUsageOverview extends Models.ItSystemUsage.IItSystemUsageOverviewReadModel {
        roles: Array<string>;
    }

    export class OverviewController implements IOverviewController {
        private storageKey = "it-system-overview-options";

        mainGrid: Kitos.IKendoGrid<IItSystemUsageOverview>;
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
            private readonly systemRoles: Array<any>,
            private readonly user,
            private readonly orgUnits: Array<any>,
            kendoGridLauncherFactory: Utility.KendoGrid.IKendoGridLauncherFactory) {
            $rootScope.page.title = "IT System - Overblik";

            //Build and launch kendo grid
            var launcher =
                kendoGridLauncherFactory
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
                        //TODO: Missing param fix for boolean parameters - they dont work out of the box for filtering
                        return parameterMap;
                    })
                    .withResponseParser(response => {
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
                            .withDataSourceName("Name")
                            .withTitle("IT System")
                            .withId("sysname")
                            .withStandardWidth(320)
                            .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                            .withRendering(dataItem => `<a data-ui-sref='it-system.usage.main({id: ${dataItem.SourceEntityId}})'>${Helpers.SystemNameFormat.apply(dataItem.Name, dataItem.ItSystemDisabled)}</a>`)
                            .withExcelOutput(dataItem => dataItem.Name))
                    .withStandardSorting("Name");

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
