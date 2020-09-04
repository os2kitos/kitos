module Kitos.DataProcessing.Agreement.Overview {
    "use strict";
    import IDataProcessingAgreement = Models.DataProcessing.IDataProcessingAgreement;
    

    export interface IOverviewController {
        mainGrid: any;
        mainGridOptions: kendo.ui.GridOptions;
        saveGridProfile(): void;
        loadGridProfile(): void;
        clearGridProfile(): void;
        doesGridProfileExist(): boolean;
        clearOptions(): void;
        createDataProcessingAgreement() :void;
    }

    export class OverviewController implements IOverviewController {
        private storageKey = "data-processing-agreement-overview-options";
        private gridState = this.gridStateService.getService(this.storageKey, this.user.id);
        public mainGrid: any;
        public mainGridOptions: kendo.ui.GridOptions;
        public canCreate: boolean;
        public projectIdToAccessLookup = {};

        public static $inject: Array<string> = [
            "$rootScope",
            "$scope",
            "$timeout",
            "$window",
            "$state",
            "$",
            "_",
            "notify",
            "user",
            "gridStateService",
            "exportGridToExcelService",
            "userAccessRights"
        ];

        constructor(
            private $rootScope: IRootScope,
            private $scope: ng.IScope,
            private $timeout: ng.ITimeoutService,
            private $window: ng.IWindowService,
            private $state: ng.ui.IStateService,
            private $: JQueryStatic,
            private _: ILoDashWithMixins,
            private notify,
            private user,
            private gridStateService: Services.IGridStateFactory,
            private exportGridToExcelService,
            private userAccessRights: Models.Api.Authorization.EntitiesAccessRightsDTO) {
            this.$rootScope.page.title = "Databehandleraftaler - Overblik"; //TODO: To service or something

            //TODO: Calls for generalization
            this.$scope.$on("kendoWidgetCreated", (event, widget) => {
                // the event is emitted for every widget; if we have multiple
                // widgets in this controller, we need to check that the event
                // is for the one we're interested in.
                if (widget === this.mainGrid) {

                    this.loadGridOptions();
                    // show loadingbar when export to excel is clicked
                    // hidden again in method exportToExcel callback
                    $(".k-grid-excel").click(() => {
                        kendo.ui.progress(this.mainGrid.element, true);
                    });
                }
            });

            //Defer until page change is complete
            setTimeout(() => this.activate(), 1);
        }

        // saves grid state to localStorage
        //TODO: Calls for generalization
        private saveGridOptions = () => {
            this.gridState.saveGridOptions(this.mainGrid);
        }

        // Resets the scrollbar position
        //TODO: Calls for generalization
        private onPaging = () => {
            Utility.KendoGrid.KendoGridScrollbarHelper.resetScrollbarPosition(this.mainGrid);
        }

        // loads kendo grid options from localstorage
        //TODO: Calls for generalization
        private loadGridOptions() {
            this.mainGrid.options.toolbar.push({ name: "excel", text: "Eksportér til Excel", className: "pull-right" });
            this.gridState.loadGridOptions(this.mainGrid);
        }

        //TODO: Calls for generalization
        public saveGridProfile() {
            this.gridState.saveGridProfile(this.mainGrid);
            this.notify.addSuccessMessage("Filtre og sortering gemt");
        }

        //TODO: Calls for generalization
        public loadGridProfile() {
            this.gridState.loadGridProfile(this.mainGrid);
            this.mainGrid.dataSource.read();
            this.notify.addSuccessMessage("Anvender gemte filtre og sortering");
        }

        //TODO: Calls for generalization
        public clearGridProfile() {
            this.gridState.removeProfile();
            this.gridState.removeSession();
            this.notify.addSuccessMessage("Filtre og sortering slettet");
            this.reload();
        }

        //TODO: Calls for generalization
        public doesGridProfileExist() {
            return this.gridState.doesGridProfileExist();
        }

        // clears grid filters by removing the localStorageItem and reloading the page
        //TODO: Calls for generalization
        public clearOptions() {
            this.gridState.removeProfile();
            this.gridState.removeLocal();
            this.gridState.removeSession();
            this.notify.addSuccessMessage("Sortering, filtering og kolonnevisning, -bredde og –rækkefølge nulstillet");
            // have to reload entire page, as dataSource.read() + grid.refresh() doesn't work :(
            this.reload();
        }

        public createDataProcessingAgreement() {
            this.$state.go("data-processing.overview.create-agreement");
        }

        private reload() {
            this.$state.go(".", null, { reload: true });
        }

        private activate() {
            this.canCreate = this.userAccessRights.canCreate;
            const mainGridOptions: IKendoGridOptions<IDataProcessingAgreement> = {
                autoBind: false, // disable auto fetch, it's done in the kendoRendered event handler
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: (options) => {
                                return `/odata/Organizations(${this.user.currentOrganizationId})/DataProcessingAgreementReadModels`;
                            },
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
                    schema: {
                        model: {
                            fields: {
                                StatusDate: { type: "date" },
                                LastChanged: { type: "date" },
                                IsTransversal: { type: "boolean" },
                                IsStrategy: { type: "boolean" },
                                IsArchived: { type: "boolean" }
                            },
                        },
                        parse: response => response
                    }
                },
                //TODO: Calls for generalization
                toolbar: [
                    {
                        //TODO: call a function which transitions to the create dialog
                        name: "createDataProcessing",
                        text: "Opret Databehandleraftale",
                        template:
                            "<button data-element-type='createDataProcessingAgreementButton' ng-click='vm.createDataProcessingAgreement()' class='btn btn-success pull-right' data-ng-disabled=\"!vm.canCreate\">#: text #</button>"
                    },
                    {
                        name: "clearFilter",
                        text: "Nulstil",
                        template:
                            "<button type='button' class='k-button k-button-icontext' title='Nulstil sortering, filtering og kolonnevisning, -bredde og –rækkefølge' data-ng-click='vm.clearOptions()'>#: text #</button>"
                    },
                    {
                        name: "saveFilter",
                        text: "Gem filter",
                        template:
                            '<button type="button" class="k-button k-button-icontext" title="Gem filtre og sortering" data-ng-click="vm.saveGridProfile()">#: text #</button>'
                    },
                    {
                        name: "useFilter",
                        text: "Anvend filter",
                        template:
                            '<button type="button" class="k-button k-button-icontext" title="Anvend gemte filtre og sortering" data-ng-click="vm.loadGridProfile()" data-ng-disabled="!vm.doesGridProfileExist()">#: text #</button>'
                    },
                    {
                        name: "deleteFilter",
                        text: "Slet filter",
                        template:
                            "<button type='button' class='k-button k-button-icontext' title='Slet filtre og sortering' data-ng-click='vm.clearGridProfile()' data-ng-disabled='!vm.doesGridProfileExist()'>#: text #</button>"
                    }
                ],
                excel: {
                    fileName: "Databehandleraftaler Overblik.xlsx",
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
                    mode: "row",
                    messages: {
                        isTrue: "✔",
                        isFalse: "✖"
                    }
                },
                groupable: false,
                columnMenu: true,
                height: this.$window.innerHeight - 200,
                dataBound: this.saveGridOptions,
                columnResize: this.saveGridOptions,
                columnHide: this.saveGridOptions,
                columnShow: this.saveGridOptions,
                columnReorder: this.saveGridOptions,
                excelExport: this.exportToExcel,
                page: this.onPaging,
                columns: [
                    {
                        field: "Name",
                        title: "Databehandleraftale",
                        attributes: {
                            "data-element-type": "dataProcessingAgreementNameKendoObject"
                        },
                        width: 340,
                        persistId: "dpaName",
                        template: dataItem => `<a data-ui-sref="data-processing.edit-agreement.main({id: ${dataItem.SourceEntityId}})">${dataItem.Name}</a>`,
                        excelTemplate: dataItem => dataItem && dataItem.Name || "",
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

            // assign the generated grid options to the scope value, kendo will do the rest
            this.mainGridOptions = mainGridOptions;
        }

        private exportToExcel = (e: IKendoGridExcelExportEvent<IDataProcessingAgreement>) => {
            this.exportGridToExcelService.getExcel(e, this._, this.$timeout, this.mainGrid);
        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", $stateProvider => {
                $stateProvider.state("data-processing.overview", {
                    url: "/overview",
                    templateUrl: "app/components/data-processing/data-processing-agreement-overview.view.html",
                    controller: OverviewController,
                    controllerAs: "vm",
                    resolve: { 
                        user: [
                            "userService", userService => userService.getUser()
                        ],
                        userAccessRights: ["authorizationServiceFactory", (authorizationServiceFactory: Services.Authorization.IAuthorizationServiceFactory) =>
                            authorizationServiceFactory 
                                .createDataProcessingAgreementAuthorization()
                                .getOverviewAuthorization()]
                    }
                });
            }
        ]);
}
