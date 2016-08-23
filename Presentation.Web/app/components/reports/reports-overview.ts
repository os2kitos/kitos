module Kitos.Reports.Overview {
    'use strict';

    export interface IOverviewController {
        mainGrid: IKendoGrid<IReportOverview>;
        mainGridOptions: kendo.ui.GridOptions;
        categorySelectorOptions: kendo.ui.DropDownListOptions;
    }

    export interface IReportOverview {
        Name: string;
        Description: string;
    }

    export class ReportsOverviewController {
        public title: string;
        public mainGrid: Kitos.IKendoGrid<any>;
        public mainGridOptions: kendo.ui.GridOptions;

        static $inject: Array<string> = [
            "$rootScope",
            "$scope",
            "$http",
            "$timeout",
            "$window",
            "$state",
            "$",
            "_",
            "moment",
            "notify",
            "user",
            "reports"
        ];

        constructor(private $rootScope: Kitos.IRootScope,
            private $scope: ng.IScope,
            private $http: ng.IHttpService,
            private $timeout: ng.ITimeoutService,
            private $window: ng.IWindowService,
            private $state: ng.ui.IStateService,
            private $: JQueryStatic,
            private _: Kitos.ILoDashWithMixins,
            private moment: moment.MomentStatic,
            private notify,
            private user,
            public reports) {

            this.$rootScope.page.title = "Raport Oversigt";

            $scope.$on("kendoWidgetCreated", (event, widget) => {
                // the event is emitted for every widget; if we have multiple
                // widgets in this controller, we need to check that the event
                // is for the one we're interested in.
                if (widget === this.mainGrid) {
                    this.mainGrid.dataSource.read();
                }
            });
            this.setupGrid();
        }

        private setupGrid() {
            var baseUrl = "odata/reports",
                dataSource = new kendo.data.DataSource({
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: baseUrl,
                            dataType: "json"
                        },
                        update: {
                            url: (data) => {
                                return baseUrl + "(" + data.Id + ")";
                            },
                            type: "PATCH"
                        },
                        destroy: {
                            url: (data) => {
                                return baseUrl + "(" + data.Id + ")";
                            },
                            type: "DELETE"
                        },
                        create: {
                            url: baseUrl,
                            type: "POST"
                        },
                        parameterMap: (data, type) => {
                            if (type === "update") {
                                var model: any = data;
                                let patch = {
                                    Name: model.Name,
                                    Description: model.Description
                                }
                                return JSON.stringify(patch);
                            }
                            if (type === "create") {
                                var model: any = data;
                                let patch = {
                                    Id: 0,
                                    Name: model.Name,
                                    Description: model.Description,
                                    OrganizationId: this.user.currentOrganizationId
                                }
                                return JSON.stringify(patch)
                            }
                        }
                    },
                    batch: false,
                    serverPaging: true,
                    serverSorting: true,
                    pageSize: 5,
                    schema: {
                        model: {
                            id: "Id",
                            fields: {
                                Id: { editable: false, nullable: true },
                                Name: { validation: { required: true } },
                                Description: { validation: { required: true } }
                            }
                        }
                    }
                });

            this.mainGridOptions = ({
                autoBind: false,
                dataSource: dataSource,
                editable: "inline",
                height: 550,
                toolbar: ["create"],
                pageable: {
                    refresh: true,
                    pageSizes: [50, 100, 200],
                    buttonCount: 5
                },
                sortable: {
                    mode: "single"
                },
                reorderable: true,
                resizable: true,
                groupable: false,
                columnMenu: {
                    filterable: true
                },
                columns: [
                    {
                        field: "Name", title: "Navn", width: 150,
                        template: dataItem => dataItem.Id ? `<a ui-sref="reports.viewer({id:${dataItem.Id}})">${dataItem.Name}</a>` : ""
                    },
                    { field: "Description", title: "Beskrivelse", width: "250px" },
                    { command: ["edit", "destroy"], title: "&nbsp;", width: "250px" }
                ]
            });
        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
                $stateProvider.state("reports.overview", {
                    url: "/overblik",
                    templateUrl: "app/components/reports/reports-overview.html",
                    controller: ReportsOverviewController,
                    controllerAs: "vm",
                    resolve: {
                        user: ["userService", userService => userService.getUser()],
                        reports: ["reportService", (rpt) => rpt.GetAll().then(result => result.data.value)]
                    }
                });
            }
        ]);

}