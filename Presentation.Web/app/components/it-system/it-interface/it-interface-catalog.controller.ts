module Kitos.ItInterface.Catalog {
    "use strict";

    export interface ICatalogController {
        mainGrid: IKendoGrid<Models.ItSystem.IItInterface>;
        mainGridOptions: IKendoGridOptions<Models.ItSystem.IItInterface>;

        saveGridProfile(): void;
        loadGridProfile(): void;
        clearGridProfile(): void;
        doesGridProfileExist(): boolean;
        clearOptions(): void;
    }

    export class CatalogController implements ICatalogController {
        private storageKey = "it-interface-catalog-options";
        private gridState = this.gridStateService.getService(this.storageKey);
        public mainGrid: IKendoGrid<Models.ItSystem.IItInterface>;
        public mainGridOptions: IKendoGridOptions<Models.ItSystem.IItInterface>;

        public canCreate: boolean;

        public static $inject: Array<string> = [
            "$rootScope",
            "$scope",
            "$timeout",
            "$state",
            "$",
            "_",
            "moment",
            "notify",
            "user",
            "gridStateService",
            "$uibModal",
            "$http",
            "needsWidthFixService"
        ];

        constructor(
            private $rootScope: IRootScope,
            private $scope: ng.IScope,
            private $timeout: ng.ITimeoutService,
            private $state: ng.ui.IStateService,
            private $: JQueryStatic,
            private _: ILoDashWithMixins,
            private moment: moment.MomentStatic,
            private notify,
            private user,
            private gridStateService: Services.IGridStateFactory,
            private $modal,
            private $http,
            private needsWidthFixService) {
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

                    // show loadingbar when export to excel is clicked
                    // hidden again in method exportToExcel callback
                    $(".k-grid-excel").click(() => {
                        kendo.ui.progress(this.mainGrid.element, true);
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
            this.canCreate = !this.user.isReadOnly;

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
                                // replaces "contains(Uuid,'11')" with "contains(CAST(Uuid, 'Edm.String'),'11')"
                                parameterMap.$filter = parameterMap.$filter.replace(/contains\(Uuid,/, "contains(CAST(Uuid, 'Edm.String'),");
                                if (!user.isGlobalAdmin) {
                                    parameterMap.$filter = parameterMap.$filter + "and Disabled eq false";

                                }
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
                        },
                        parse: response => {
                            // iterrate each usage
                            this._.forEach(response.value, ItInterface => {
                                if (!ItInterface.InterfaceType) { ItInterface.InterfaceType = { Name: "" }; }
                                if (!ItInterface.BelongsTo) { ItInterface.BelongsTo = { Name: "" }; }
                                if (!ItInterface.ExhibitedBy) { ItInterface.ExhibitedBy = { ItSystem: { Name: "" } }; }
                                if (!ItInterface.Tsa) { ItInterface.Tsa = { Name: "" }; }
                                if (!ItInterface.Interface) { ItInterface.Interface = { Name: "" }; }
                                if (!ItInterface.Method) { ItInterface.Method = { Name: "" }; }
                                if (!ItInterface.Organization) { ItInterface.Organization = { Name: "" }; }
                            });
                            return response;
                        }
                    },                
                    pageSize: 100,
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true
                },
                toolbar: [
                    {
                        name: "createSnitflade",
                        text: "Opret Snitflade",
                        template: "<button ng-click='interfaceCatalogVm.createSnitflade()' data-element-type='createInterfaceButton' class='btn btn-success pull-right' data-ng-disabled=\"!interfaceCatalogVm.canCreate\">#: text #</button>"
                    },
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
                    pageSizes: [10, 25, 50, 100, 200, "all"],
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
                columnMenu: true,
                height: window.innerHeight - 200,
                dataBound: this.saveGridOptions,
                columnResize: this.saveGridOptions,
                columnHide: this.saveGridOptions,
                columnShow: this.saveGridOptions,
                columnReorder: this.saveGridOptions,
                excelExport: this.exportToExcel,
                page: this.onPaging,
                columns: [
                    {
                        field: "ItInterfaceId", title: "Snitflade ID", width: 120,
                        persistId: "infid", // DON'T YOU DARE RENAME!
                        excelTemplate: dataItem => dataItem && dataItem.ItInterfaceId || "",
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Name", title: "Snitflade", width: 285,
                        persistId: "name", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            if (dataItem.Disabled) {
                                return `<a data-element-type='InterfaceName' data-ui-sref='it-system.interface-edit.main({id: ${dataItem.Id}})'>${dataItem.Name} (Slettes)</a>`;
                            } else {
                                return `<a data-element-type='InterfaceName' data-ui-sref='it-system.interface-edit.main({id: ${dataItem.Id}})'>${dataItem.Name}</a>`;
                            }
                        },
                        excelTemplate: dataItem => {
                            if (dataItem && dataItem.Name) {
                                if (dataItem.Disabled) {
                                    return dataItem.Name + " (Slettes)";
                                } else {
                                    return dataItem.Name;
                                }
                            } else {
                                return "";
                            }
                        },
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Version", title: "Version", width: 150,
                        persistId: "version", // DON'T YOU DARE RENAME!
                        excelTemplate: dataItem => dataItem && dataItem.Version || "",
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
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
                        filterable: {
                            cell: {
                                showOperators: false,
                                template: this.accessModFilter
                            }
                        }
                    },
                    {
                        field: "InterfaceType.Name", title: "Snitfladetype", width: 150,
                        persistId: "inftype", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.InterfaceType ? dataItem.InterfaceType.Name : "",
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "BelongsTo.Name", title: "Rettighedshaver", width: 150,
                        persistId: "belongs", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.BelongsTo ? dataItem.BelongsTo.Name : "",
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
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
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "ExhibitedBy.ItSystem.Name", title: "Udstillet af", width: 230,
                        persistId: "exhibit", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            if (dataItem.ExhibitedBy && dataItem.ExhibitedBy.ItSystem.Name)
                                return (dataItem.ExhibitedBy.ItSystem.Disabled) ? dataItem.ExhibitedBy.ItSystem.Name + " (Slettes)" : dataItem.ExhibitedBy.ItSystem.Name;
                            else
                                return "";
                        },
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Tsa.Name", title: "TSA", width: 90,
                        persistId: "tsa", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.Tsa ? dataItem.Tsa.Name : "",
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Interface.Name", title: "Grænseflade", width: 150,
                        persistId: "infname", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.Interface ? dataItem.Interface.Name : "",
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Method.Name", title: "Metode", width: 150,
                        persistId: "method", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.Method ? dataItem.Method.Name : "",
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "DataType", title: "Datatype", width: 150,
                        persistId: "datatypes", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            var value = "";
                            if (dataItem.DataRows.length > 0) {
                                value = this._.map(dataItem.DataRows.slice(0, 4), "DataType.Name").join(", ");
                            }
                            if (dataItem.DataRows.length > 5) {
                                value += ", ...";
                            }
                            return value;
                        },
                        excelTemplate: dataItem => {
                            var value = "";
                            if (dataItem && dataItem.DataRows.length > 0) {
                                value = this._.map(dataItem.DataRows, "DataType.Name").join(", ");
                            }
                            return value;
                        },
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Organization.Name", title: "Oprettet af: Organisation", width: 150,
                        persistId: "orgname", // DON'T YOU DARE RENAME!
                        excelTemplate: dataItem => dataItem && dataItem.Organization && dataItem.Organization.Name || "",
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
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
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "LastChangedByUser.Name", title: "Sidst redigeret: Bruger", width: 150,
                        persistId: "lastchangedname", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.LastChangedByUser && `${dataItem.LastChangedByUser.Name} ${dataItem.LastChangedByUser.LastName}` || "",
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
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
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    }
                ]
            };
            function customFilter(args) {
                args.element.kendoAutoComplete({
                    noDataTemplate: ''
                });
            }
        }


        public createSnitflade() {
            var self = this;
            var modalInstance = this.$modal.open({
                // fade in instead of slide from top, fixes strange cursor placement in IE
                // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                windowClass: "modal fade in",
                templateUrl: "app/components/it-system/it-interface/it-interface-modal-create.view.html",
                controller: ["$scope", "$uibModalInstance", function ($scope, $modalInstance) {
                    $scope.formData = { itInterfaceId: "" }; // set itInterfaceId to an empty string
                    $scope.type = "IT Snitflade";
                    $scope.checkAvailbleUrl = "api/itInterface/";


                    $scope.validateName = function () {
                        $scope.createForm.name.$validate();
                    }
                    $scope.validateItInterfaceId = function () {
                        $scope.createForm.itInterfaceId.$validate();
                    }

                    $scope.uniqueConstraintError = false;

                    $scope.submit = function () {
                        var payload = {
                            name: $scope.formData.name,
                            itInterfaceId: $scope.formData.itInterfaceId,
                            belongsToId: self.user.currentOrganizationId,
                            organizationId: self.user.currentOrganizationId
                        };

                        var msg = self.notify.addInfoMessage("Opretter snitflade...", false);
                        self.$http.post("api/itinterface", payload)
                            .success(function (result) {
                                msg.toSuccessMessage("En ny snitflade er oprettet!");
                                var interfaceId = result.response.id;
                                $modalInstance.close(interfaceId);
                            }).error(function () {
                                msg.toErrorMessage("Fejl! Kunne ikke oprette snitflade!");
                            });
                    };
                }]
            });

            modalInstance.result.then(function (id) {
                // modal was closed with OK
                self.$state.go("it-system.interface-edit.main", { id: id });
            });
        }


        // saves grid state to localStorage
        private saveGridOptions = () => {
            this.gridState.saveGridOptions(this.mainGrid);
        }

        // Resets the position of the scrollbar
        private onPaging = () => {
            Utility.ScrollBarHelper.resetScrollbarPosition(this.mainGrid);
        }

        // loads kendo grid options from localstorage
        private loadGridOptions() {
            //Add only excel option if user is not readonly
            if (!this.user.isReadOnly) {
                this.mainGrid.options.toolbar.push({ name: "excel", text: "Eksportér til Excel", className: "pull-right" });
            }
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

        private exportFlag = false;
        private exportToExcel = (e: IKendoGridExcelExportEvent<Models.ItSystem.IItInterface>) => {
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
    }

    angular
        .module("app")
        .config([
            "$stateProvider", $stateProvider => {
                $stateProvider.state("it-system.interfaceCatalog", {
                    url: "/interface-catalog",
                    templateUrl: "app/components/it-system/it-interface/it-interface-catalog.view.html",
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
