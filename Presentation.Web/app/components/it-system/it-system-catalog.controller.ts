module Kitos.ItSystem.Catalog {
    "use strict";

    export interface ICatalogController {
        mainGrid: IKendoGrid<Models.ItSystem.IItSystem>;
        mainGridOptions: kendo.ui.GridOptions;
        usageGrid: kendo.ui.Grid;
        usageDetailsGrid: kendo.ui.GridOptions;
        modal: kendo.ui.Window;

        saveGridProfile(): void;
        loadGridProfile(): void;
        clearGridProfile(): void;
        doesGridProfileExist(): boolean;
        clearOptions(): void;
        enableUsage(dataItem): void;
        removeUsage(dataItem): void;
    }

    export class CatalogController implements ICatalogController {
        private storageKey = "it-system-catalog-options";
        private gridState = this.gridStateService.getService(this.storageKey);
        public mainGrid: IKendoGrid<Models.ItSystem.IItSystem>;
        public mainGridOptions: IKendoGridOptions<Models.ItSystem.IItSystem>;
        public usageGrid: kendo.ui.Grid;
        public modal: kendo.ui.Window;

        public static $inject: Array<string> = [
            "$rootScope",
            "$scope",
            "$http",
            "$timeout",
            "$state",
            "$sce",
            "$",
            "_",
            "moment",
            "notify",
            "user",
            "gridStateService"
        ];

        constructor(
            private $rootScope: IRootScope,
            private $scope: ng.IScope,
            private $http: ng.IHttpService,
            private $timeout: ng.ITimeoutService,
            private $state: ng.ui.IStateService,
            private $sce: ng.ISCEService,
            private $: JQueryStatic,
            private _: ILoDashWithMixins,
            private moment: moment.MomentStatic,
            private notify,
            private user,
            private gridStateService: Services.IGridStateFactory) {
            $rootScope.page.title = "IT System - Katalog";

            $scope.$on("kendoWidgetCreated", (event, widget) => {
                // the event is emitted for every widget; if we have multiple
                // widgets in this controller, we need to check that the event
                // is for the one we're interested in.
                if (widget === this.mainGrid) {
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
                }
            });

            var itSystemBaseUrl: string;
            if (user.isGlobalAdmin) {
                // global admin should see all it systems everywhere with all levels of access
                itSystemBaseUrl = "/odata/ItSystems";
            } else {
                // everyone else are limited to within organizationnal context
                itSystemBaseUrl = `/odata/Organizations(${user.currentOrganizationId})/ItSystems`;
            }
            var itSystemUrl = itSystemBaseUrl + "?$expand=AppTypeOption,BusinessType,BelongsTo,TaskRefs,Parent,Organization,ObjectOwner,Usages($expand=Organization),LastChangedByUser";

            // catalog grid
            this.mainGridOptions = {
                autoBind: false, // disable auto fetch, it's done in the kendoRendered event handler
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: itSystemUrl,
                            dataType: "json"
                        },
                        parameterMap: (options, type) => {
                            var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);

                            if (parameterMap.$filter) {
                                // replaces 'Kitos.AccessModifier0' with Kitos.AccessModifier'0'
                                parameterMap.$filter = parameterMap.$filter.replace(/('Kitos\.AccessModifier([0-9])')/, "Kitos.AccessModifier'$2'");
                                // replaces "startswith(TaskKey,'11')" with "TaskRefs/any(c: startswith(c/TaskKey),'11')"
                                parameterMap.$filter = parameterMap.$filter.replace(/(\w+\()(TaskKey.*\))/, "TaskRefs/any(c: $1c/$2)");
                                // replaces "startswith(TaskName,'11')" with "TaskRefs/any(c: startswith(c/Description),'11')"
                                parameterMap.$filter = parameterMap.$filter.replace(/(\w+\()TaskName(.*\))/, "TaskRefs/any(c: $1c/Description$2)");
                                // replaces "contains(Uuid,'11')" with "contains(CAST(Uuid, 'Edm.String'),'11')"
                                parameterMap.$filter = parameterMap.$filter.replace(/contains\(Uuid,/, "contains(CAST(Uuid, 'Edm.String'),");
                            }

                            return parameterMap;
                        }
                    },
                    sort: {
                        field: "Name",
                        dir: "asc"
                    },
                    schema: {
                        model: {
                            fields: {
                                LastChanged: { type: "date" }
                            }
                        }
                    },
                    pageSize: 100,
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true
                },
                toolbar: [
                    { name: "excel", text: "Eksportér til Excel", className: "pull-right" },
                    {
                        name: "clearFilter",
                        text: "Nulstil",
                        template: "<button type='button' class='k-button k-button-icontext' title='Nulstil sortering, filtering og kolonnevisning, -bredde og –rækkefølge' data-ng-click='systemCatalogVm.clearOptions()'>#: text #</button>"
                    },
                    {
                        name: "saveFilter",
                        text: "Gem filter",
                        template: '<button type="button" class="k-button k-button-icontext" title="Gem filtre og sortering" data-ng-click="systemCatalogVm.saveGridProfile()">#: text #</button>'
                    },
                    {
                        name: "useFilter",
                        text: "Anvend filter",
                        template: '<button type="button" class="k-button k-button-icontext" title="Anvend gemte filtre og sortering" data-ng-click="systemCatalogVm.loadGridProfile()" data-ng-disabled="!systemCatalogVm.doesGridProfileExist()">#: text #</button>'
                    },
                    {
                        name: "deleteFilter",
                        text: "Slet filter",
                        template: "<button type='button' class='k-button k-button-icontext' title='Slet filtre og sortering' data-ng-click='systemCatalogVm.clearGridProfile()' data-ng-disabled='!systemCatalogVm.doesGridProfileExist()'>#: text #</button>"
                    }
                ],
                excel: {
                    fileName: "IT System Katalog.xlsx",
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
                filterable: {
                    mode: "row"
                },
                groupable: false,
                columnMenu: {
                    filterable: false
                },
                dataBound: this.saveGridOptions,
                columnResize: this.saveGridOptions,
                columnHide: this.saveGridOptions,
                columnShow: this.saveGridOptions,
                columnReorder: this.saveGridOptions,
                excelExport: this.exportToExcel,
                columns: [
                    {
                        field: "Usages", title: "Anvend/Fjern anvendelse", width: 100,
                        persistId: "command", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            // true if system is being used by system within current context, else false
                            var systemHasUsages = this._.find(dataItem.Usages, (d: any) => (d.OrganizationId == this.user.currentOrganizationId));

                            if (systemHasUsages)
                                return `<button type="button" class="btn btn-danger col-md-7" data-ng-click="systemCatalogVm.removeUsage(${dataItem.Id})">Fjern anv.</button>`;

                            return `<button type="button" class="btn btn-success col-md-7" data-ng-click="systemCatalogVm.enableUsage(${dataItem.Id})">Anvend</button>`;
                        },
                        excelTemplate: dataItem => {
                            // true if system is being used by system within current context, else false
                            var systemHasUsages = dataItem ? this._.find(dataItem.Usages, (d: any) => (d.OrganizationId == this.user.currentOrganizationId)) : false;
                            return systemHasUsages ? "Anvendt" : "Ikke anvendt";
                        },
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "Parent.Name", title: "Overordnet IT System", width: 150,
                        persistId: "parentname", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.Parent ? dataItem.Parent.Name : "",
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Name", title: "It System", width: 285,
                        persistId: "name", // DON'T YOU DARE RENAME!
                        template: dataItem => `<a data-ui-sref="it-system.edit.interfaces({id: ${dataItem.Id}})">${dataItem.Name}</a>`,
                        excelTemplate: dataItem => dataItem && dataItem.Name || "",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "AccessModifier", title: "Synlighed", width: 120,
                        persistId: "accessmod", // DON'T YOU DARE RENAME!
                        template: `<display-access-modifier value="dataItem.AccessModifier"></display-access-modifier>`,
                        excelTemplate: dataItem => dataItem && dataItem.AccessModifier.toString() || "",
                        hidden: true,
                        filterable: {
                            cell: {
                                showOperators: false,
                                template: this.accessModFilter
                            }
                        }
                    },
                    {
                        field: "BusinessType.Name", title: "Forretningstype", width: 150,
                        persistId: "busitype", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.BusinessType ? dataItem.BusinessType.Name : "",
                        attributes: { "class": "might-overflow" },
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "AppTypeOption.Name", title: "Applikationstype", width: 150,
                        persistId: "apptype", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.AppTypeOption ? dataItem.AppTypeOption.Name : "",
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "BelongsTo.Name", title: "Rettighedshaver", width: 210,
                        persistId: "belongsto", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.BelongsTo ? dataItem.BelongsTo.Name : "",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "TaskKey", title: "KLE ID", width: 150,
                        persistId: "taskkey", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.TaskRefs.length > 0 ? this._.map(dataItem.TaskRefs, "TaskKey").join(", ") : "",
                        attributes: { "class": "might-overflow" },
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "startswith"
                            }
                        },
                        sortable: false
                    },
                    {
                        field: "TaskName", title: "KLE Navn", width: 155,
                        persistId: "taskname", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.TaskRefs.length > 0 ? this._.map(dataItem.TaskRefs, "Description").join(", ") : "",
                        attributes: { "class": "might-overflow" },
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "startswith"
                            }
                        },
                        sortable: false
                    },
                    {
                        field: "Url", title: "Link til beskrivelse", width: 125,
                        persistId: "link", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            if (!dataItem.Url) {
                                return "";
                            }
                            return `<a href="${dataItem.Url}" title="Link til yderligere..." target="_blank"><i class="fa fa-link"></i></a>`;
                        },
                        excelTemplate: dataItem => dataItem && dataItem.Url || "",
                        attributes: { "class": "text-center" },
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Usages.length", title: "IT System: Anvendes af", width: 95,
                        persistId: "usages", // DON'T YOU DARE RENAME!
                        template: dataItem =>
                            `<a class="col-md-7 text-center" data-ng-click="systemCatalogVm.showUsageDetails(${dataItem.Id},'${this.$sce.getTrustedHtml(dataItem.Name)}')">${dataItem.Usages.length}</a>`,
                        excelTemplate: dataItem => dataItem && dataItem.Usages && dataItem.Usages.length.toString() || "",
                        filterable: false,
                        sortable: false
                    },
                    //{
                    //    field: "", title: "Snitflader: Udstilles globalt", width: 95,
                    //    persistId: "globalexpsure", // DON'T YOU DARE RENAME!
                    //    template: "TODO",
                    //    hidden: true,
                    //    filterable: false,
                    //    sortable: false
                    //},
                    //{
                    //    field: "", title: "Snitflader: Anvendes globalt", width: 95,
                    //    persistId: "globalusage", // DON'T YOU DARE RENAME!
                    //    template: "TODO",
                    //    hidden: true,
                    //    filterable: false,
                    //    sortable: false
                    //},
                    {
                        field: "Organization.Name", title: "Oprettet af: Organisation", width: 150,
                        persistId: "orgname", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.Organization ? dataItem.Organization.Name : "",
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "ObjectOwner.Name", title: "Oprettet af: Bruger", width: 150,
                        persistId: "ownername", // DON'T YOU DARE RENAME!
                        template: dataItem => `${dataItem.ObjectOwner.Name} ${dataItem.ObjectOwner.LastName}`,
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "LastChangedByUser.Name", title: "Sidst redigeret: Bruger", width: 150,
                        persistId: "lastchangedname", // DON'T YOU DARE RENAME!
                        template: dataItem => `${dataItem.LastChangedByUser.Name} ${dataItem.LastChangedByUser.LastName}`,
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "LastChanged", title: "Sidst redigeret: Dato", format: "{0:dd-MM-yyyy}", width: 130,
                        persistId: "lastchangeddate", // DON'T YOU DARE RENAME!
                        excelTemplate: dataItem => {
                            // handles null cases
                            if (!dataItem || !dataItem.LastChanged) {
                                return "";
                            }

                            return this.moment(dataItem.LastChanged).format("DD-MM-YYYY");
                        },
                        attributes: { "class": "text-center" },
                        filterable: {
                            cell: {
                                showOperators: false,
                                operator: "gte"
                            }
                        }
                    },
                    {
                        field: "Uuid", title: "UUID", width: 150,
                        persistId: "uuid", // DON'T YOU DARE RENAME!
                        excelTemplate: dataItem => dataItem.Uuid,
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    }
                ]
            };
        }

        // usagedetails grid empty-grid handling
        private detailsBound(e) {
            var grid = e.sender;
            if (grid.dataSource.total() == 0) {
                var colCount = grid.columns.length;
                this.$(e.sender.wrapper)
                    .find("tbody")
                    .append(`<tr class="kendo-data-row"><td colspan="${colCount}" class="no-data text-muted">System anvendens ikke</td></tr>`);
            }
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
            // have to reload entire page, as dataSource.read() + grid.refresh() doesn't work :(
            this.reload();
        };

        private reload() {
            this.$state.go(".", null, { reload: true });
        }

        // show usageDetailsGrid - takes a itSystemUsageId for data and systemName for modal title
        public showUsageDetails = (usageId, systemName) => {
            //Filter by usageId
            this.usageGrid.dataSource.filter({ field: "ItSystemId", operator: "eq", value: usageId });
            //Set modal title
            this.modal.setOptions({ title: `Anvendelse af ${systemName}` });
            //Open modal
            this.modal.center().open();
        }

        // usagedetails grid
        public usageDetailsGrid: kendo.ui.GridOptions = {
            dataSource: {
                type: "odata-v4",
                transport:
                {
                    read: {
                        url: "/odata/ItSystemUsages?$expand=Organization",
                        dataType: "json"
                    }
                },
                serverPaging: true,
                serverSorting: true,
                serverFiltering: true
            },
            autoBind: false,
            columns: [
                {
                    field: "Organization.Name",
                    title: "Organisation"
                }
            ],
            dataBound: this.detailsBound
        };

        private exportFlag = false;
        private exportToExcel = (e: IKendoGridExcelExportEvent<Models.ItSystem.IItSystem>) => {
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

        // adds usage at selected system within current context
        public enableUsage(dataItem) {
            this.addUsage(dataItem).then(() => this.mainGrid.dataSource.fetch());
        }

        // removes usage at selected system within current context
        public removeUsage(dataItem) {
            var sure = confirm("Er du sikker på at du vil fjerne den lokale anvendelse af systemet? Dette sletter ikke systemet, men vil slette alle lokale detaljer vedrørende anvendelsen.");
            if (sure)
                this.deleteUsage(dataItem).then(() => this.mainGrid.dataSource.fetch());
        }

        // adds system to usage within the current context
        private addUsage(systemId) {
            return this.$http.post("api/itSystemUsage", {
                itSystemId: systemId,
                organizationId: this.user.currentOrganizationId
            })
                .success(() => this.notify.addSuccessMessage("Systemet er taget i anvendelse"))
                .error(() => this.notify.addErrorMessage("Systemet kunne ikke tages i anvendelse!"));
        }

        // removes system from usage within the current context
        private deleteUsage(systemId) {
            var url = `api/itSystemUsage?itSystemId=${systemId}&organizationId=${this.user.currentOrganizationId}`;

            return this.$http.delete(url)
                .success(() => this.notify.addSuccessMessage("Anvendelse af systemet er fjernet"))
                .error(() => this.notify.addErrorMessage("Anvendelse af systemet kunne ikke fjernes!"));
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
            "$stateProvider", $stateProvider => {
                $stateProvider.state("it-system.catalog", {
                    url: "/catalog",
                    templateUrl: "app/components/it-system/it-system-catalog.view.html",
                    controller: CatalogController,
                    controllerAs: "systemCatalogVm",
                    resolve: {
                        user: [
                            "userService", userService => userService.getUser()
                        ]
                    }
                });
            }
        ]);
}
