module Kitos.LocalAdmin.Organization {
    "use strict";

    export interface IOverviewController {
        mainGrid: Kitos.IKendoGrid<Models.IOrganization>;
        mainGridOptions: kendo.ui.GridOptions;
    }

    export class OrganizationController implements IOverviewController {

        private storageKey = "local-org-overview-options";
        private gridState = this.gridStateService.getService(this.storageKey, this.user);
        public mainGrid: Kitos.IKendoGrid<Models.IOrganization>;
        public mainGridOptions: kendo.ui.GridOptions;

        public static $inject: Array<string> = [
            "$rootScope",
            "$scope",
            "$timeout",
            "$state",
            "_",
            "notify",
            "gridStateService",
            "exportGridToExcelService",
            "user"
        ];

        constructor(
            private $rootScope: IRootScope,
            private $scope: ng.IScope,
            private $timeout: ng.ITimeoutService,
            private $state: ng.ui.IStateService,
            private _: ILoDashWithMixins,
            private notify,
            private gridStateService: Services.IGridStateFactory,
            private exportGridToExcelService,
            private user) {
            $rootScope.page.title = "Org overblik";

            $scope.$on("kendoWidgetCreated", (event, widget) => {
                if (widget === this.mainGrid) {
                    this.loadGridOptions();
                }
            });

            //Defer loading grid until after navigation completed
            setTimeout(() => this.activate(), 1);
        }

        private saveGridOptions = () => {
            this.gridState.saveGridOptions(this.mainGrid);
        }

        private onPaging = () => {
            Utility.KendoGrid.KendoGridScrollbarHelper.resetScrollbarPosition(this.mainGrid);
        }

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

        public clearOptions() {
            this.gridState.removeProfile();
            this.gridState.removeLocal();
            this.gridState.removeSession();
            this.notify.addSuccessMessage("Sortering, filtering og kolonnevisning, -bredde og –rækkefølge nulstillet");
            this.reload();
        };

        public generateExcel() {
            kendo.ui.progress(this.mainGrid.element, true);
        }

        private reload() {
            this.$state.go(".", null, { reload: true });
        }

        private activate() {
            var mainGridOptions: IKendoGridOptions<Models.IOrganization> = {
                autoBind: false,
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: `/odata/Organizations?$expand=Type`,
                            dataType: "json"
                        }
                    },
                    sort: {
                        field: "Name",
                        dir: "asc"
                    },
                    pageSize: 100,
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true,
                },
                toolbar: [
                    {
                        name: "opretOrganisation",
                        text: "Opret Organisation",
                        template:
                            "<a ui-sref='local-config.org.create' class='btn btn-success pull-right'>#: text #</a>"
                    },
                    {
                        name: "clearFilter",
                        text: "Gendan kolonneopsætning",
                        template:
                            "<button type='button' class='k-button k-button-icontext' title='Nulstil sortering, filtering og kolonnevisning, -bredde og –rækkefølge' data-ng-click='orgCtrl.clearOptions()' data-element-type='resetFilterButton'>#: text #</button>"
                    },
                    {
                        name: "saveFilter",
                        text: "Gem filter",
                        template:
                            "<button type='button' class='k-button k-button-icontext' title='Gem filtre og sortering' data-ng-click='orgCtrl.saveGridProfile()' data-element-type='saveFilterButton'>#: text #</button>"
                    },
                    {
                        name: "useFilter",
                        text: "Anvend filter",
                        template:
                            "<button type='button' class='k-button k-button-icontext' title='Anvend gemte filtre og sortering' data-ng-click='orgCtrl.loadGridProfile()' data-ng-disabled='!orgCtrl.doesGridProfileExist()' data-element-type='useFilterButton'>#: text #</button>"
                    },
                    {
                        name: "deleteFilter",
                        text: "Slet filter",
                        template:
                            "<button type='button' class='k-button k-button-icontext' title='Slet filtre og sortering' data-ng-click='orgCtrl.clearGridProfile()' data-ng-disabled='!orgCtrl.doesGridProfileExist()' data-element-type='removeFilterButton'>#: text #</button>"
                    },
                    {
                        name: "excel",
                        text: "Eksportér til Excel",
                        template: "<a role='button' class='k-button k-button-icontext pull-right k-grid-excel' data-ng-click='orgCtrl.generateExcel()'> <span class='k-icon k-i-file-excel'> </span> Eksportér til Excel</a>"

        }
                ],
                excel: {
                    fileName: "Organisationer.xlsx",
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
                        field: "Name", title: "Navn", width: 230,
                        persistId: "name",
                        hidden: false,
                        excelTemplate: (dataItem) => dataItem.Name,
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
                        field: "Cvr", title: "CVR", width: 230,
                        persistId: "cvr",
                        hidden: false,
                        excelTemplate: (dataItem) => dataItem.Cvr,
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
                        field: "Type.Name", title: "Type", width: 230,
                        persistId: "type",
                        hidden: false,
                        template: (dataItem) => dataItem.Type.Name,
                        excelTemplate: (dataItem) => dataItem.Type.Name,
                        filterable: {
                            cell: {
                                template: function (args) {
                                    args.element.kendoDropDownList({
                                        dataSource: [{ type: "Kommune", value: "Kommune" }, { type: "Interessefællesskab", value: "Interessefællesskab" }, { type: "Virksomhed", value: "Virksomhed" }, { type: "Anden offentlig myndighed", value: "Anden offentlig myndighed" }],
                                        dataTextField: "type",
                                        dataValueField: "value",
                                        valuePrimitive: true
                                    });
                                },
                                showOperators: false
                            }
                        }
                    },
                    {
                        field: "AccessModifier", title: "Synlighed", width: 230,
                        persistId: "synlighed",
                        hidden: false,
                        template: `<display-access-modifier value="dataItem.AccessModifier"></display-access-modifier>`,
                        excelTemplate: (dataItem) => dataItem.AccessModifier.toString(),
                        filterable: {
                            cell: {
                                template: function (args) {
                                    args.element.kendoDropDownList({
                                        dataSource: [{ type: "Lokal", value: "Local" }, { type: "Offentlig", value: "Public" }],
                                        dataTextField: "type",
                                        dataValueField: "value",
                                        valuePrimitive: true
                                    });
                                },
                                showOperators: false
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

            this.mainGridOptions = mainGridOptions;
        }

        private exportToExcel = (e: IKendoGridExcelExportEvent<Models.IOrganizationRight>) => {
            this.exportGridToExcelService.getExcel(e, this._, this.$timeout, this.mainGrid);
        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", ($stateProvider) => {
                $stateProvider.state("local-config.org", {
                    url: "/org",
                    templateUrl: "app/components/local-config/local-config-org.view.html",
                    controller: OrganizationController,
                    controllerAs: "orgCtrl",
                    authRoles: [Models.OrganizationRole.LocalAdmin],
                    resolve: {
                        user: [
                            "userService", (userService) => {
                                return userService.getUser();
                            }
                        ]
                    }
                });
            }]);
}
