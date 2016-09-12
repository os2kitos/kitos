module Kitos.Reports.Overview {
    "use strict";

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
        public currentEditModel: kendo.data.Model;

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

        private getAccessModifier = () => {
            return [
                { Id: 0, Name: "Lokal" },
                { Id: 1, Name: "Offentlig" }
            ]
        }

        private setupGrid() {
            let baseUrl = "odata/reports";
            let dataSource = {
                type: "odata-v4",
                transport: {
                    read: {
                        url: baseUrl + "?$expand=CategoryType",
                        dataType: "json"
                    },
                    update: {
                        url: (data) => {
                            return baseUrl + "(" + data.Id + ")";
                        },
                        beforeSend: function (req) {
                            req.setRequestHeader("Prefer", "return=representation")
                        },
                        type: "PATCH",
                        dataType: "json"
                    },
                    destroy: {
                        url: (data) => {
                            return baseUrl + "(" + data.Id + ")";
                        },
                        type: "DELETE",
                        dataType: "json"
                    },
                    create: {
                        url: baseUrl,
                        type: "POST",
                        dataType: "json"
                    },
                    parameterMap: (data, type) => {
                        // Call the default OData V4 parameterMap. 
                        var result = kendo.data.transports["odata-v4"].parameterMap(data, type);
                        if (type == "read") {
                            result.$count = true;
                        }

                        if (type === "update") {
                            let model: any = data;
                            let patch = {
                                Name: model.Name,
                                Description: model.Description,
                                CategoryTypeId: model.CategoryTypeId,
                                AccessModifier: model.AccessModifier

                            };
                            return JSON.stringify(patch);
                        }

                        if (type === "create") {
                            let model: any = data;
                            let patch = {
                                Id: 0,
                                Name: model.Name,
                                Description: model.Description,
                                CategoryTypeId: model.CategoryTypeId,
                                AccessModifier: model.AccessModifier
                            };
                            return JSON.stringify(patch);
                        }
                        return result;
                    }
                },
                sort: {
                    field: "Name",
                    dir: "asc"
                },
                batch: false,
                sync: false,
                serverPaging: true,
                serverSorting: true,
                serverFiltering: true,
                pageSize: 5,
                schema: {
                    model: {
                        id: "Id",
                        fields: {
                            Id: { editable: false, nullable: true },
                            Name: { editable: true, validation: { required: true } },
                            Description: { editable: true, validation: { required: true } },
                            CategoryTypeId: { type: "number" },
                            AccessModifier: { type: "string" }
                        }
                    }
                }
            };

            this.mainGridOptions = {
                autoBind: false,
                dataSource: dataSource,
                editable: "inline",
                height: 550,
                toolbar: ["create"],
                edit: (e) => {
                    this.currentEditModel = e.model;
                },
                pageable: {
                    refresh: true,
                    pageSizes: [5],
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
                        template: dataItem => dataItem.Id ? `<a href='appReport/?id=${dataItem.Id}' target='_blank'>${dataItem.Name}</a>` : ""
                    },
                    { field: "Description", title: "Beskrivelse", width: "250px" },
                    {
                        field: "CategoryTypeId", title: "Category", width: "180px", editor: this.CategoryDropDownEditor,
                        template: dataitem => dataitem.CategoryType ? dataitem.CategoryType.Name : ""
                    },
                    {
                        field: "AccessModifier", title: "Adgang", width: "60px", editor: this.AccessModifierEditor,
                        template: dataitem => {
                            switch (dataitem.AccessModifier) {
                                case "Local": return "Lokal";
                                case "Public": return "Offentlig";
                                default: return "";
                            }
                        }
                    },
                    { command: ["edit", "destroy"], title: "&nbsp;", width: "250px" }
                ]

            };
        }

        AccessModifierEditor = (container, options) => {
            this.$(`<input required name="" + options.field + ""/>`)
                .appendTo(container)
                .kendoDropDownList({
                    autoBind: true,
                    dataTextField: "Name",
                    dataValueField: "Id",
                    dataBound: (e) => {
                        let edata = e;
                        this.$timeout((edata) => {
                            var catId = this.currentEditModel.get("AccessModifier");
                            edata.sender.select((dataItem) => {
                                return dataItem.Id === catId;
                            })
                        });
                    },
                    dataSource: this.getAccessModifier(),
                    change: (e: kendo.ui.DropDownListChangeEvent) => {
                        var newValue = e.sender.dataItem();
                        var cur = this.currentEditModel;
                        cur.set("AccessModifier", newValue.Id);
                    }
                });
        };

        CategoryDropDownEditor = (container, options) => {
            this.$(`<input required name="" + options.field + ""/>`)
                .appendTo(container)
                .kendoDropDownList({
                    autoBind: true,
                    dataTextField: "Name",
                    dataValueField: "Id",
                    dataBound: (e) => {
                        var catId = this.currentEditModel.get("CategoryTypeId");
                        e.sender.select((dataItem) => {
                            return dataItem.Id === catId;
                        })
                    },
                    dataSource: {
                        type: "odata-v4",
                        transport: {
                            read: "odata/ReportCategories"
                        }
                    },

                    change: (e: kendo.ui.DropDownListChangeEvent) => {
                        var newValue = e.sender.dataItem();
                        var cur = this.currentEditModel;
                        cur.set("CategoryType", newValue);
                        cur.set("CategoryTypeId", newValue.Id);
                    }
                });
        };
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
