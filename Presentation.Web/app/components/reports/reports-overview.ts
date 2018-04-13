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
        private canCreate: boolean;
        private storageKey = "report-overview-options";
        private gridState = this.gridStateService.getService(this.storageKey);
        public categoryTypeValues;

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
            "$confirm",
            "globalConfig",
            "gridStateService",
            "reportCategoryTypes",
            "needsWidthFixService"
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
            private $confirm,
            private globalConfigs,
            private gridStateService: Services.IGridStateFactory,
            private reportCategoryTypes,
            private needsWidthFixService) {
            
            this.$rootScope.page.title = "Rapport Oversigt";

            var canGlobalAdminOnlyEditReports = _.find(this.globalConfigs, function (g: any) {
                return g.key === "CanGlobalAdminOnlyEditReports";
            });

            this.canCreate = (canGlobalAdminOnlyEditReports.value === "true") ? user.isGlobalAdmin : user.isGlobalAdmin || user.isLocalAdmin || user.isReportAdmin;

            if (!user.isGlobalAdmin && user.isReadOnly){
                this.canCreate = false;
            }

            this.categoryTypeValues = [];
            var self = this;
            this._.each(this.reportCategoryTypes, function (value) {
                self.categoryTypeValues.push({ text: value.Name, value: value.Id });
            });

            $scope.$on("kendoWidgetCreated", (event, widget) => {
                this.loadGridOptions();
                this.mainGrid.dataSource.read();

                // find the access modifier filter row section
                var accessModifierFilterRow = $(".k-filter-row [data-field='AccessModifier']");
                // find the access modifier kendo widget
                var accessModifierFilterWidget = accessModifierFilterRow.find("input").data("kendoDropDownList");
                // attach a click event to the X (remove filter) button
                accessModifierFilterRow.find("button").on("click", () => {
                    // set the selected filter to none, because clicking the button removes the filter
                    accessModifierFilterWidget.select(0);
                });

                // show loadingbar when export to excel is clicked
                // hidden again in method exportToExcel callback
                $(".k-grid-excel").click(() => {
                    kendo.ui.progress(this.mainGrid.element, true);
                });
            });
            this.setupGrid();
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
                    this.mainGrid.dataSource.remove(dataItem);
                    this.mainGrid.dataSource.sync();
                });
        }

        private getAccessModifier = () => {
            return [
                { Id: 0, Name: "Lokal" },
                { Id: 1, Name: "Offentlig" }
            ];
        }

        private setupGrid() {
            let baseUrl = "odata/reports";
            let dataSource = {
                type: "odata-v4",
                transport: {
                    read: {
                        url: function () {
                            return baseUrl + "?$expand=CategoryType,Organization,LastChangedByUser($select=Id,Name,LastName)";
                        },
                        dataType: "json"
                    },
                    update: {
                        url: (data) => {
                            return baseUrl + "(" + data.Id + ")";
                        },
                        beforeSend: function (req) {
                            req.setRequestHeader("Prefer", "return=representation");
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
                        if (result) {

                            if (result.$filter) {
                                // replaces 'Kitos.AccessModifier0' with Kitos.AccessModifier'0'
                                result.$filter = result.$filter.replace(/('Kitos\.AccessModifier([0-9])')/, "Kitos.AccessModifier'$2'");
                            }

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
                columnMenu: true,
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
                            AccessModifier: { type: "string" },
                            CategoryTypeId: { type: "number" }
                        }
                    }
                }
            };

            this.mainGridOptions = {
                autoBind: false,
                dataBinding: (e) => {
                    let currentOrganizationId = this.user.currentOrganizationId;

                    for (let i = 0; i < e.items.length; i++) {
                        e.items[i].canEdit = (this.user.isGlobalAdmin || this.canCreate && e.items[i].OrganizationId == currentOrganizationId);
                    }
                },
                dataSource: dataSource,
                editable: "popup",
                toolbar: [
                    {
                        name: "createReport",
                        template:
                            `<button type="button" class="btn btn-success pull-right" title="Opret rapport" data-ng-click="vm.onCreate()" data-ng-disabled="!vm.canCreate">Opret rapport</button>`
                    },
                    { name: "excel", text: "Eksportér til Excel", className: "pull-right" },
                    {
                        name: "clearFilter",
                        text: "Nulstil",
                        template: "<button type='button' class='k-button k-button-icontext' title='Nulstil sortering, filtering og kolonnevisning, -bredde og –rækkefølge' data-ng-click='vm.clearOptions()'>#: text #</button>"
                    },
                    {
                        name: "saveFilter",
                        text: "Gem filter",
                        template: '<button type="button" class="k-button k-button-icontext" title="Gem filtre og sortering" data-ng-click="vm.saveGridProfile()">#: text #</button>'
                    },
                    {
                        name: "useFilter",
                        text: "Anvend filter",
                        template: '<button type="button" class="k-button k-button-icontext" title="Anvend gemte filtre og sortering" data-ng-click="vm.loadGridProfile()" data-ng-disabled="!vm.doesGridProfileExist()">#: text #</button>'
                    },
                    {
                        name: "deleteFilter",
                        text: "Slet filter",
                        template: "<button type='button' class='k-button k-button-icontext' title='Slet filtre og sortering' data-ng-click='vm.clearGridProfile()' data-ng-disabled='!vm.doesGridProfileExist()'>#: text #</button>"
                    }
                ],
                excel: {
                    fileName: "Rapport Overblik.xlsx",
                    filterable: true,
                    allPages: true
                },
                pageable: {
                    refresh: true,
                    pageSizes: [10, 25, 50, 100, 200],
                    buttonCount: 5
                },
                sortable: {
                    mode: "single"
                },
                reorderable: true,
                resizable: true,
                groupable: false,
                filterable: {
                    mode: "row",
                },
                columnMenu: true,
                height: 900,
                dataBound: this.saveGridOptions,
                columnResize: this.saveGridOptions,
                columnHide: this.saveGridOptions,
                columnShow: this.saveGridOptions,
                columnReorder: this.saveGridOptions,
                excelExport: this.exportToExcel,
                columns: [
                    {
                        field: "Name",
                        persistId: "name",
                        title: "Navn",
                        width: 150,
                        template: dataItem => dataItem.Id
                            ? `<a href='appReport/?id=${dataItem.Id}' target='_blank'>${dataItem.Name}</a>`
                            : "",
                        excelTemplate: dataItem => dataItem.Name,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    } as any,
                    {
                        field: "Description",
                        persistId: "desc",
                        title: "Beskrivelse",
                        width: "250px",
                        excelTemplate: dataItem => dataItem.Description,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "CategoryTypeId",
                        persistId: "catTypeId",
                        title: "Kategori",
                        width: "180px",
                        menu: false,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "eq"
                            }
                        },
                        values: this.categoryTypeValues
                    },
                    {
                        field: "AccessModifier",
                        persistId: "accessModifier",
                        title: "Synlighed",
                        width: "150px",
                        template: `<display-access-modifier value="dataItem.AccessModifier"></display-access-modifier>`,
                        filterable: {
                            cell: {
                                showOperators: false,
                                template: this.accessModFilter
                            }
                        }
                    },
                    {
                        field: "Organization.Name",
                        persistId: "org",
                        title: "Organisation",
                        width: "60px",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "LastChanged",
                        persistId: "lastChanged",
                        title: "Sidst ændret",
                        width: "60px",
                        type: "date",
                        filterable: false,
                        template: dataitem => kendo.toString(kendo.parseDate(dataitem.LastChanged), "d")
                    },
                    {
                        field: "LastChangedByUser.Name",
                        persistId: "lastChangedByUser",
                        title: "Sidst ændret af",
                        width: "150px",
                        filterable: false,
                        template: dataitem => dataitem.LastChangedByUser.Name + " " + dataitem.LastChangedByUser.LastName
                    },
                    {
                        title: "Handlinger",
                        persistId: "actions",
                        width: 70,
                        filterable: false,
                        template: dataItem => `<button type='button' class='btn btn-link' title='Redigér rapport' data-ng-click='vm.onEdit($event)' data-ng-disabled='!${dataItem.canEdit}'><i class='fa fa-pencil' aria-hidden='true'></i></button>` +
                                              `<button type='button' class='btn btn-link' title='Slet rapport' data-ng-click='vm.onDelete($event)' data-ng-disabled='!${dataItem.canEdit}'><i class='fa fa-minus' aria-hidden='true'></i></button>`
                    }
                ]
            };
        }

        // saves grid state to localStorage
        private saveGridOptions = () => {
            this.gridState.saveGridOptions(this.mainGrid);
        }

        // loads kendo grid options from localstorage
        private loadGridOptions() {
            this.gridState.loadGridOptions(this.mainGrid);
        }

        public saveGridProfile = () => {
            this.gridState.saveGridProfile(this.mainGrid);
            this.notify.addSuccessMessage("Filtre og sortering gemt");
        }

        public loadGridProfile() {
            this.gridState.loadGridProfile(this.mainGrid);
            this.mainGrid.dataSource.read();
            this.notify.addSuccessMessage("Anvender gemte filtre og sortering");
        }

        public clearGridProfile() {
            this.gridState.removeProfile();
            this.gridState.removeSession();
            this.notify.addSuccessMessage("Filtre og sortering slettet");
            this.reload();
        };

        public doesGridProfileExist = () => {
            return this.gridState.doesGridProfileExist();
        };

        // clears grid filters by removing the localStorageItem and reloading the page
        public clearOptions() {
            this.gridState.removeProfile();
            this.gridState.removeLocal();
            this.gridState.removeSession();
            this.notify.addSuccessMessage("Sortering, filtering og kolonnevisning, -bredde og –rækkefølge nulstillet");
            // have to reload entire page, as dataSource.read() + grid.refresh() doesn't work :( works with this.loadGridOptions() and this.mainGrid.dataSource.read();
            this.reload();
        };

        private reload() {
            this.$state.go(".", null, { reload: true });
        }

        private exportFlag = false;
        private exportToExcel = (e: IKendoGridExcelExportEvent<Models.ItProject.IItProject>) => {
            var columns = e.sender.columns;

            if (!this.exportFlag) {
                e.preventDefault();
                this._.forEach(columns, column => {
                    if (column.hidden) {
                        column.tempVisual = true;
                        e.sender.showColumn(column);
                    }
                });
                this.$timeout(() => {
                    this.exportFlag = true;
                    e.sender.saveAsExcel();
                });
            } else {
                this.exportFlag = false;

                // hide coloumns on visual grid
                this._.forEach(columns, column => {
                    if (column.tempVisual) {
                        delete column.tempVisual;
                        e.sender.hideColumn(column);
                    }
                });

                // render templates
                var sheet = e.workbook.sheets[0];

                // skip header row
                for (var rowIndex = 1; rowIndex < sheet.rows.length; rowIndex++) {
                    var row = sheet.rows[rowIndex];

                    // -1 as sheet has header and dataSource doesn't
                    var dataItem = e.data[rowIndex - 1];

                    for (var columnIndex = 0; columnIndex < row.cells.length; columnIndex++) {
                        if (columns[columnIndex].field === "") continue;
                        var cell = row.cells[columnIndex];

                        var template = this.getTemplateMethod(columns[columnIndex]);

                        cell.value = template(dataItem);
                    }
                }

                // hide loadingbar when export is finished
                kendo.ui.progress(this.mainGrid.element, false);
                this.needsWidthFixService.fixWidth();
            }
        }

        private getTemplateMethod(column) {
            var template: Function;

            if (column.excelTemplate) {
                template = column.excelTemplate;
            } else if (typeof column.template === "function") {
                template = <Function>column.template;
            } else if (typeof column.template === "string") {
                template = kendo.template(<string>column.template);
            } else {
                template = t => t;
            }

            return template;
        }

        private accessModFilter = (args) => {
            var self = this;
            var gridDataSource = args.dataSource;

            function setSelected() {
                var kendoElem = this;
                var currentFilter = gridDataSource.filter();
                var filterObj = self._.findKeyDeep(currentFilter, { field: "AccessModifier" });

                switch (filterObj.value) {
                    case "Kitos.AccessModifier0":
                        kendoElem.select(1);
                        break;
                    case "Kitos.AccessModifier1":
                        kendoElem.select(2);
                        break;
                    default:
                        kendoElem.select(0); // select placeholder
                }
            }

            function applyFilter() {
                var kendoElem = this;
                // can't use args.dataSource directly,
                // if we do then the view doesn't update.
                // So have to go through $scope - sadly :(
                var dataSource = self.mainGrid.dataSource;
                var selectedValue = kendoElem.value();
                var field = "AccessModifier";
                var currentFilter = dataSource.filter();
                // remove old value first
                var newFilter = self._.removeFiltersForField(currentFilter, field);

                if (selectedValue) {
                    newFilter = self._.addFilter(newFilter, field, "eq", selectedValue, "and");
                }

                dataSource.filter(newFilter);
            }

            // http://dojo.telerik.com/ODuDe/5
            args.element.removeAttr("data-bind");
            args.element.kendoDropDownList({
                dataSource: [
                    { value: "Kitos.AccessModifier0", text: "Lokal" },
                    { value: "Kitos.AccessModifier1", text: "Offentlig" }
                ],
                dataTextField: "text",
                dataValueField: "value",
                optionLabel: "Vælg filter...",
                dataBound: setSelected,
                change: applyFilter
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
                        reports: ["reportService", (rpt) => rpt.GetAll().then(result => result.data.value)],
                        globalConfig: [
                            "$http", $http => $http.get("/odata/GlobalConfigs").then(result => result.data.value)
                        ],
                        reportCategoryTypes: [
                            "reportService", reportService => reportService.getReportCategories().then(result => result.data.value)
                        ]
                    }
                });
            }
        ]);

}
