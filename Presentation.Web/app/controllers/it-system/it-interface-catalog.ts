module Kitos.ItInterface.Catalog {
    "use strict";

    export interface ICatalogController {
        mainGrid: Kitos.IKendoGrid;
        mainGridOptions: Kitos.IKendoGridOptions;

        saveGridProfile(): void;
        loadGridProfile(): void;
        clearGridProfile(): void;
        doesGridProfileExist(): boolean;
        clearOptions(): void;
    }

    export class CatalogController implements ICatalogController {
        private storageKey = "it-interface-catalog-options";
        private gridState = this.gridStateService.getService(this.storageKey);
        public mainGrid: Kitos.IKendoGrid;
        public mainGridOptions: Kitos.IKendoGridOptions;

        private $inject: Array<string> = [
            "$rootScope",
            "$scope",
            "$timeout",
            "$state",
            "$",
            "_",
            "notify",
            "user",
            "gridStateService"
        ];

        constructor(
            private $rootScope: Kitos.IRootScope,
            private $scope: ng.IScope,
            private $timeout: ng.ITimeoutService,
            private $state: ng.ui.IStateService,
            private $: JQueryStatic,
            private _: Kitos.ILodashWithMixins,
            private notify,
            private user,
            private gridStateService: Kitos.Services.IGridStateFactory) {
            $rootScope.page.title = "Snitflade - Katalog";

            $scope.$on("kendoWidgetCreated", (event, widget) => {
                // the event is emitted for every widget; if we have multiple
                // widgets in this controller, we need to check that the event
                // is for the one we're interested in.
                if (widget === this.mainGrid) {
                    this.loadGridOptions();
                    this.mainGrid.dataSource.read();

                    // find the access modifier filter row section
                    var accessModifierFilterRow = this.$(".k-filter-row [data-field='AccessModifier']");
                    // find the access modifier kendo widget
                    var accessModifierFilterWidget = accessModifierFilterRow.find("input").data("kendoDropDownList");
                    // attach a click event to the X (remove filter) button
                    accessModifierFilterRow.find("button").on("click", () => {
                        // set the selected filter to none, because clicking the button removes the filter
                        accessModifierFilterWidget.select(0);
                    });
                }
            });

            var itInterfaceBaseUrl: string;
            if (user.isGlobalAdmin) {
                // global admin should see all it systems everywhere with all levels of access
                itInterfaceBaseUrl = "/odata/ItInterfaces";
            } else {
                // everyone else are limited to within organizationnal context
                itInterfaceBaseUrl = `/odata/Organizations(${user.currentOrganizationId})/ItInterfaces`;
            }

            var itInterfaceUrl = itInterfaceBaseUrl + "?$expand=Interface,InterfaceType,ObjectOwner,BelongsTo,Organization,Tsa,ExhibitedBy($expand=ItSystem),Method,LastChangedByUser,DataRows($expand=DataType),InterfaceLocalUsages";

            this.mainGridOptions = {
                autoBind: false, // disable auto fetch, it's done in the kendoRendered event handler
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: itInterfaceUrl,
                            dataType: "json"
                        },
                        parameterMap: (options, type) => {
                            var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);

                            if (parameterMap.$filter) {
                                // replaces 'Kitos.AccessModifier0' with Kitos.AccessModifier'0'
                                parameterMap.$filter = parameterMap.$filter.replace(/('Kitos\.AccessModifier([0-9])')/, "Kitos.AccessModifier'$2'");
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
                    serverFiltering: true,
                },
                toolbar: [
                    { name: "excel", text: "Eksportér til Excel", className: "pull-right" },
                    {
                        name: "clearFilter",
                        text: "Nulstil",
                        template: "<button type='button' class='k-button k-button-icontext' title='Nulstil sortering, filtering og kolonnevisning, -bredde og –rækkefølge' data-ng-click='interfaceCatalogVm.clearOptions()'>#: text #</button>"
                    },
                    {
                        name: "saveFilter",
                        text: "Gem filter",
                        template: '<button type="button" class="k-button k-button-icontext" title="Gem filtre og sortering" data-ng-click="interfaceCatalogVm.saveGridProfile()">#: text #</button>'
                    },
                    {
                        name: "useFilter",
                        text: "Anvend filter",
                        template: '<button type="button" class="k-button k-button-icontext" title="Anvend gemte filtre og sortering" data-ng-click="interfaceCatalogVm.loadGridProfile()" data-ng-disabled="!interfaceCatalogVm.doesGridProfileExist()">#: text #</button>'
                    },
                    {
                        name: "deleteFilter",
                        text: "Slet filter",
                        template: "<button type='button' class='k-button k-button-icontext' title='Slet filtre og sortering' data-ng-click='interfaceCatalogVm.clearGridProfile()' data-ng-disabled='!interfaceCatalogVm.doesGridProfileExist()'>#: text #</button>"
                    }
                ],
                excel: {
                    fileName: "Snitflade Katalog.xlsx",
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
                        field: "ItInterfaceId", title: "Snidtflade ID", width: 120,
                        persistId: "infid", // DON'T YOU DARE RENAME!
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "Name", title: "Snitflade", width: 285,
                        persistId: "name", // DON'T YOU DARE RENAME!
                        template: "<a data-ui-sref='it-system.interface-edit.interface-details({id: #: Id #})'>#: Name #</a>",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "Version", title: "Version", width: 150,
                        persistId: "version", // DON'T YOU DARE RENAME!
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "AccessModifier", title: "Synlighed", width: 120,
                        persistId: "accessmod", // DON'T YOU DARE RENAME!
                        filterable: {
                            cell: {
                                showOperators: false,
                                template: this.accessModFilter,
                            }
                        }
                    },
                    {
                        field: "InterfaceType.Name", title: "Snitfladetype", width: 150,
                        persistId: "inftype", // DON'T YOU DARE RENAME!
                        template: "#: InterfaceType ? InterfaceType.Name : '' #",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "BelongsTo.Name", title: "Rettighedshaver", width: 150,
                        persistId: "belongs", // DON'T YOU DARE RENAME!
                        template: "#: BelongsTo ? BelongsTo.Name : '' #",
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "Url", title: "Link til beskrivelse", width: 125,
                        persistId: "link", // DON'T YOU DARE RENAME!
                        template: this.linkTemplate,
                        attributes: { "class": "text-center" },
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "ExhibitedBy.ItSystem.Name", title: "Udstillet af", width: 230,
                        persistId: "exhibit", // DON'T YOU DARE RENAME!
                        template: "#: ExhibitedBy ? ExhibitedBy.ItSystem.Name : '' #",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        },
                    },
                    //{ TODO
                    //    field: "", title: "Snitflader: Anvendes globalt", width: 115,
                    //    persistId: "infglobalusage", // DON'T YOU DARE RENAME!
                    //    template: "#: InterfaceLocalUsages.length #",
                    //    filterable: {
                    //        cell: {
                    //            dataSource: [],
                    //            showOperators: false,
                    //            operator: "contains",
                    //        }
                    //    },
                    //},
                    {
                        field: "Tsa.Name", title: "TSA", width: 90,
                        persistId: "tsa", // DON'T YOU DARE RENAME!
                        template: "#: Tsa ? Tsa.Name : '' #",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        },
                    },
                    {
                        field: "Interface.Name", title: "Grænseflade", width: 150,
                        persistId: "infname", // DON'T YOU DARE RENAME!
                        template: "#: Interface ? Interface.Name : '' #",
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "Method.Name", title: "Metode", width: 150,
                        persistId: "method", // DON'T YOU DARE RENAME!
                        template: "#: Method ? Method.Name : '' #",
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        },
                    },
                    {
                        field: "", title: "Datatype", width: 150,
                        persistId: "datatypes", // DON'T YOU DARE RENAME!
                        template: "#: DataRows.length > 0 ? _.pluck(DataRows.slice(0,4), 'DataType.Name').join(', ') : '' ##: DataRows.length > 5 ? ', ...' : '' #",
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        },
                    },
                    {
                        field: "Organization.Name", title: "Oprettet af: Organisation", width: 150,
                        persistId: "orgname", // DON'T YOU DARE RENAME!
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "ObjectOwner.Name", title: "Oprettet af: Bruger", width: 150,
                        persistId: "ownername", // DON'T YOU DARE RENAME!
                        template: "#: ObjectOwner.Name + ' ' + ObjectOwner.LastName #",
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "LastChangedByUser.Name", title: "Sidst redigeret: Bruger", width: 150,
                        persistId: "lastchangedname", // DON'T YOU DARE RENAME!
                        template: "#: LastChangedByUser.Name + ' ' + LastChangedByUser.LastName #",
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "LastChanged", title: "Sidst redigeret: Dato", format: "{0:dd-MM-yyyy}", width: 130,
                        persistId: "lastchangeddate", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        filterable: {
                            cell: {
                                showOperators: false,
                                operator: "gte"
                            }
                        }
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

        public saveGridProfile() {
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
        }

        public doesGridProfileExist() {
            return this.gridState.doesGridProfileExist();
        }

        // clears grid filters by removing the localStorageItem and reloading the page
        public clearOptions() {
            this.gridState.removeProfile();
            this.gridState.removeLocal();
            this.gridState.removeSession();
            this.notify.addSuccessMessage("Sortering, filtering og kolonnevisning, -bredde og –rækkefølge nulstillet");
            // have to reload entire page, as dataSource.read() + grid.refresh() doesn't work :(
            this.reload();
        }

        private reload() {
            this.$state.go(".", null, { reload: true });
        }

        private linkTemplate(dataItem) {
            if (dataItem.Url)
                return `<a href="${dataItem.Url}" title="Link til yderligere..." target="_blank"><i class="fa fa-link"></i></a>`;
            return "";
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
                    case "Kitos.AccessModifier2":
                        kendoElem.select(3);
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

            // advice from kendo support http://dojo.telerik.com/ODuDe/5
            args.element.removeAttr("data-bind");
            args.element.kendoDropDownList({
                dataSource: [
                    { value: "Kitos.AccessModifier0", text: "Normal" },
                    { value: "Kitos.AccessModifier1", text: "Public" },
                    { value: "Kitos.AccessModifier2", text: "Private" }
                ],
                dataTextField: "text",
                dataValueField: "value",
                optionLabel: "Vælg filter...",
                dataBound: setSelected,
                change: applyFilter
            });
        }

        private exportFlag = false;
        private exportToExcel = (e) => {
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
                this._.forEach(columns, column => {
                    if (column.tempVisual) {
                        delete column.tempVisual;
                        e.sender.hideColumn(column);
                    }
                });
            }
        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", $stateProvider => {
                $stateProvider.state("it-system.interfaceCatalog", {
                    url: "/interface-catalog",
                    templateUrl: "partials/it-system/it-interface-catalog.html",
                    controller: CatalogController,
                    controllerAs: "interfaceCatalogVm",
                    resolve: {
                        user: [
                            "userService", userService => userService.getUser()
                        ]
                    }
                });
            }
        ]);
}
