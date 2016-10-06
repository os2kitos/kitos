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
        private canDelete: boolean = true;
        private canCreate: boolean = true;
        private canEdit: boolean = true;

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
            "reports",
            "$confirm"
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
            private user: Services.IUser,
            public reports,
            private $confirm) {

            this.$rootScope.page.title = "Rapport Oversigt";

            $scope.$on("kendoWidgetCreated", (event, widget) => {
                // the event is emitted for every widget; if we have multiple
                // widgets in this controller, we need to check that the event
                // is for the one we're interested in.
                if (widget === this.mainGrid) {
                    this.mainGrid.dataSource.read();
                }
            });
            this.setUpSequrity();
            this.setupGrid();
        }

        private setUpSequrity = () => {
            let canAll = this.user.isGlobalAdmin || this.user.isLocalAdmin || this.user.isReportAdmin;
            this.canCreate = this.canDelete = this.canEdit = canAll;
        }

        public onEdit = (e: JQueryEventObject) => {
            e.preventDefault();
            var dataItem = this.mainGrid.dataItem(this.$(e.currentTarget).closest("tr"));
            var entityId = dataItem["Id"];
            this.$state.go("reports.overview.report-edit", { id: entityId });
        }

        public onCreate = () => {
            this.$state.go("reports.overview.report-edit", { id: 0 });
        }

        public onDelete = (e: JQueryEventObject) => {
            e.preventDefault();
            this.$confirm({ text: 'Ønsker du at slette rapporten?', title: 'Slet rapport', ok: 'Ja', cancel: 'Nej' })
                .then(() => {
                    var dataItem = this.mainGrid.dataItem(this.$(e.currentTarget).closest("tr"));
                    var entityId = dataItem["Id"];
                    this.mainGrid.dataSource.remove(dataItem);
                    this.mainGrid.dataSource.sync();
                });
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
                columnMenu: false,
                serverPaging: true,
                serverSorting: true,
                serverFiltering: true,
                groupable: false,
                pageSize: 20,
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
                editable: "popup",
                height: 550,
                toolbar: [
                    {
                        name: "createReport",
                        template: `<button type="button" class="btn btn-success" title="Opret rapport" data-ng-click="vm.onCreate()" data-ng-disabled="!vm.canCreate">Opret rapport</button>`
                    }
                ],
                edit: (e) => {
                    this.currentEditModel = e.model;
                },
                pageable: {
                    refresh: true,
                    pageSizes: [20],
                    buttonCount: 20
                },
                sortable: {
                    mode: "single"
                },
                reorderable: true,
                resizable: true,
                groupable: false,
                columns: [
                    {
                        field: "Name", title: "Navn", width: 150, menu: false,
                        template: dataItem => dataItem.Id ? `<a href='appReport/?id=${dataItem.Id}' target='_blank'>${dataItem.Name}</a>` : ""
                    } as any,
                    { field: "Description", title: "Beskrivelse", width: "250px", menu: false },
                    {
                        field: "CategoryTypeId", title: "Category", width: "180px", menu: false,
                        template: dataitem => dataitem.CategoryType ? dataitem.CategoryType.Name : ""
                    },
                    {
                        field: "AccessModifier", title: "Adgang", width: "60px", menu: false,
                        template: dataitem => {
                            switch (dataitem.AccessModifier) {
                                case "Local": return "Lokal";
                                case "Public": return "Offentlig";
                                default: return "";
                            }
                        }
                    },
                    {
                        command: [
                            // {
                            //     name: "editReport",
                            //     template: "<button type='button' class='btn btn-link' title='Redigér rapport' data-ng-click='vm.onEdit()' data-ng-disabled='!vm.canEdit'><i class='fa fa-pencil' aria-hidden='true'></i></button>"
                            // } as any,
                            // {
                            //     name: "deleteReport",
                            //     template: "<button type='button' class='btn btn-link' title='Slet rapport' data-ng-click='vm.onDelete()' data-ng-disabled='!vm.canDelete'><i class='fa fa-minus' style='color:darkred' aria-hidden='true'></i></button>"
                            // } as any,
                            {
                                name: "editReport",
                                buttonType: "Image",
                                title: "Redigér rapport",
                                text: "",
                                imageClass: "fa fa-pencil",
                                className: "btn btn-link",
                                iconClass: "fa",
                                click: this.onEdit
                            } as any,
                            {
                                name: "deleteReport",
                                buttonType: "Image",
                                title: "Slet rapport",
                                text: "",
                                imageClass: "fa fa-minus",
                                className: "btn btn-link",
                                iconClass: "fa",
                                style: "color:darkred",
                                click: this.onDelete
                            } as any

                        ],
                        title: "Handlinger", name: "handlinger", width: 70
                    }
                ]
            };
            if (!this.canDelete || !this.canEdit)
                this.mainGridOptions.columns.pop();
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
