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
            var crudServiceBaseUrl = "odata/reports",
            dataSource = new kendo.data.DataSource({
                type: "odata-v4",
                transport: {
                    read: {
                        url: crudServiceBaseUrl,
                        dataType: "jsonp"
                    },
                    update: {
                        url: crudServiceBaseUrl,
                        dataType: "jsonp"
                    },
                    destroy: {
                        url: crudServiceBaseUrl,
                        dataType: "jsonp"
                    },
                    create: {
                        url: crudServiceBaseUrl,
                        dataType: "jsonp"
                    },
                    parameterMap: function (options, operation) {
                        if (operation !== "read" && options.models) {
                            return { models: kendo.stringify(options.models) };
                        }
                    }
                },
                batch: true,
                pageSize: 20,
                schema: {
                    model: {
                        id: "Id",
                        fields: {
                            Id: { editable: false, nullable: true },
                            Name: { validation: { required: true } },
                            Note: { validation: { required: true } }
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
                    { field: "ProductName", title: "Navn", width: "120px" },
                    { field: "Note", title: "Note", width: "250px" },
                    { command: ["edit", "destroy"], title: "&nbsp;", width: "250px" }],
                editable: "inline"
            });
        }

        private activate() {
            var mainGridOptions: IKendoGridOptions<IReportOverview> = {
                autoBind: true, // disable auto fetch, it's done in the kendoRendered event handler
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: (options) => {
                                var urlParameters = `?$expand=Parent,ResponsibleOrganizationUnit,PaymentModel,PaymentFreqency,Rights($expand=User,Role),Supplier,AssociatedSystemUsages($expand=ItSystemUsage($expand=ItSystem)),TerminationDeadline,ContractSigner`;
                                return `/odata/Reports`;// + urlParameters;
                            }
                        },
                        dataType: "json"
                    },
                    sort: {
                        field: "Name",
                        dir: "asc"
                    },
                    pageSize: 100,
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true
                }
            }
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
                        reports: ["reportService", (rpt) => rpt.GetAll().then(result => result.data.value)]
                    }
                });
            }
        ]);

}