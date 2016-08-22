module Kitos.Reports.Overview {
    'use strict';

    export interface IOverviewController {
        mainGrid: IKendoGrid<IReportOverview>;
        mainGridOptions: kendo.ui.GridOptions;
        categorySelectorOptions: kendo.ui.DropDownListOptions;
    }

    export interface IReportOverview {
        Name: string;
        Note: string;
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
                                return baseUrl + "(" + data.models[0].Id + ")";
                            },
                            dataType: "json",
                            type: "PATCH"
                        },
                        destroy: {
                            url: (data) => {
                                return baseUrl + "(" + data.Id + ")";
                            },
                            dataType: "json",
                            type: "DELETE"
                        },
                        create: {
                            url: baseUrl,
                            dataType: "json",
                            type: "POST"
                        },
                        parameterMap: (options, type) => {
                            if(type === "update") {
                                var model: any = options.models[0];
                                let patch = {
                                    Name: model.Name,
                                    Description: model.Description
                                }
                                return JSON.stringify(patch);     
                            }
                            if(type === "create") {
                                var model: any = options.models[0];
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
                        total: function (data) { return data['@@odata.count']; },
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
                dataSource: dataSource,
                pageable: true,
                height: 550,
                toolbar: ["create"],
                columns: [
                    { field: "Name", title: "Navn", width: "115px" },
                    { field: "Description", title: "Beskrivelse", width: "250px" },
                    { command: ["edit", "destroy"], title: "&nbsp;", width: "250px" }],
                editable: "inline"
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