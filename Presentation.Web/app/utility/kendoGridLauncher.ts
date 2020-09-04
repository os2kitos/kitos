module Kitos.Utility.KendoGrid {
    import IDataProcessingAgreement = Models.DataProcessing.IDataProcessingAgreement;

    export interface IGridViewAccess<TDataSource> {
        mainGrid: IKendoGrid<TDataSource>;
        mainGridOptions: IKendoGridOptions<TDataSource>;
    }

    export interface IKendoGridLauncher<TDataSource> {
        launch() : void;
        withAccessRights(entitiesAccessRightsDto: Models.Api.Authorization.EntitiesAccessRightsDTO): IKendoGridLauncher<TDataSource>;
        withUser(user: Services.IUser): IKendoGridLauncher<TDataSource>;
        withGridBinding(gridBinding: IGridViewAccess<TDataSource>): IKendoGridLauncher<TDataSource>;
        withEntityTypeName(name: string): IKendoGridLauncher<TDataSource>;
        withStorageKey(newKey: string): IKendoGridLauncher<TDataSource>;
        withScope($scope: ng.IScope): IKendoGridLauncher<TDataSource>;
    }

    export class KendoGridLauncher<TDataSource> implements IKendoGridLauncher<TDataSource>{
        private $scope: ng.IScope = null;
        private storageKey: string = null;
        private gridState: Services.IGridStateService = null;
        private accessRights: Models.Api.Authorization.EntitiesAccessRightsDTO = <Models.Api.Authorization.EntitiesAccessRightsDTO>{};
        private entityTypeName: string = null;
        private gridBinding: IGridViewAccess<TDataSource> = null;
        private user: Services.IUser = null;
        public canCreate: boolean = false;

        constructor(
            private gridStateService: Services.IGridStateFactory,
            private exportGridToExcelService: Services.System.ExportGridToExcelService,
            private notify,
            private $state: ng.ui.IStateService,
            private $timeout: ng.ITimeoutService,
            private _: ILoDashWithMixins,
            private $: JQueryStatic,
            private $window: ng.IWindowService
        ) {

        }

        withScope($scope: ng.IScope) : IKendoGridLauncher<TDataSource> {
            if (!$scope) throw "$scope must be defined";
            this.$scope = $scope;
            return this;
        }

        withUser(user: Services.IUser): IKendoGridLauncher<TDataSource> {
            if (!user) throw "user must be defined";
            this.user = user;
            return this;
        }

        withGridBinding(gridBinding: IGridViewAccess<TDataSource>): IKendoGridLauncher<TDataSource> {
            if(!gridBinding) throw "gridBinding must be defined";
            this.gridBinding = gridBinding;
            return this;
        }

        withEntityTypeName(name: string): IKendoGridLauncher<TDataSource> {
            if (!name) throw "name must be defined";
            this.entityTypeName = name;
            return this;
        }

        withStorageKey(newKey: string): IKendoGridLauncher<TDataSource> {
            if (!newKey) {
                throw "Storage key must be defined";
            }
            this.storageKey = newKey;
            this.gridState = this.gridStateService.getService(this.storageKey, this.user.id);
            return this;
        }

        withAccessRights(rights: Models.Api.Authorization.EntitiesAccessRightsDTO): IKendoGridLauncher<TDataSource> {
            this.accessRights = rights;
            return this;
        }

        // saves grid state to localStorage
        private saveGridOptions = () => {
            this.gridState.saveGridOptions(this.gridBinding.mainGrid);
        }

        // Resets the scrollbar position
        private onPaging = () => {
            Utility.KendoGrid.KendoGridScrollbarHelper.resetScrollbarPosition(this.gridBinding.mainGrid);
        }

        // loads kendo grid options from localstorage
        private loadGridOptions() {
            this.gridBinding.mainGrid.options.toolbar.push({ name: "excel", text: "Eksportér til Excel", className: "pull-right" });
            this.gridState.loadGridOptions(this.gridBinding.mainGrid);
        }

        saveGridProfile() {
            this.gridState.saveGridProfile(this.gridBinding.mainGrid);
            this.notify.addSuccessMessage("Filtre og sortering gemt");
        }

        loadGridProfile() {
            this.gridState.loadGridProfile(this.gridBinding.mainGrid);
            this.gridBinding.mainGrid.dataSource.read();
            this.notify.addSuccessMessage("Anvender gemte filtre og sortering");
        }

        clearGridProfile() {
            this.gridState.removeProfile();
            this.gridState.removeSession();
            this.notify.addSuccessMessage("Filtre og sortering slettet");
            this.reload();
        }

        doesGridProfileExist() {
            //TODO: Does not provide the right answer
            return this.gridState.doesGridProfileExist();
        }

        // clears grid filters by removing the localStorageItem and reloading the page
        clearOptions() {
            //TODO: Some controllers have more in this
            this.gridState.removeProfile();
            this.gridState.removeLocal();
            this.gridState.removeSession();
            this.notify.addSuccessMessage("Sortering, filtering og kolonnevisning, -bredde og –rækkefølge nulstillet");
            // have to reload entire page, as dataSource.read() + grid.refresh() doesn't work :(
            this.reload();
        }

        createNewItemInstance() {
            //TODO - perhaps a configurable button in stead of the precise buttons
            this.$state.go("data-processing.overview.create-agreement");
        }

        private reload() {
            //TODO
            //this.$state.go(".", null, { reload: true });
        }

        private exportToExcel = (e: IKendoGridExcelExportEvent<TDataSource>) => {
            this.exportGridToExcelService.getExcel(e, this._, this.$timeout, this.gridBinding.mainGrid);
        }

        private build() {
            //TODO: Check required fields
            //TODO: Refactor content of this to use TDataSource in stead and column creation as well + url

            this.canCreate = this.accessRights.canCreate;
            this.$scope.vm.canCreate = this.canCreate;
            this.$scope.vm.createNewItemInstance = this.createNewItemInstance;
            this.$scope.vm.clearOptions = this.clearOptions;
            this.$scope.vm.saveGridProfile = this.saveGridProfile;
            this.$scope.vm.loadGridProfile = this.loadGridProfile;
            this.$scope.vm.clearGridProfile = this.clearGridProfile;
            this.$scope.vm.doesGridProfileExist = this.doesGridProfileExist;

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
                toolbar: [
                    //TODO: Make this configurable as well as the custom new elements
                    { //TODO: Perhaps the "create button" os optionally injected hence should not be manually handled in here in the generic part
                        name: "createDataProcessing",
                        text: `Opret ${this.entityTypeName}`,
                        template:
                            "<button data-element-type='createDataProcessingAgreementButton' ng-click='vm.createNewItemInstance()' class='btn btn-success pull-right' data-ng-disabled=\"!vm.canCreate\">#: text #</button>"
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
                    fileName: `${this.entityTypeName}  Overblik.xlsx`,
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
                    { //TODO: To column builder
                        field: "Name",
                        title: "Databehandleraftale",
                        attributes: {
                            "data-element-type": "dataProcessingAgreementNameKendoObject"
                        },
                        width: 340,
                        persistId: "dpaName",
                        template: dataItem => `<a data-ui-sref="data-processing.overview.edit-agreement.main({id: ${dataItem.SourceEntityId}})">${dataItem.Name}</a>`,
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
            //TODO: Remove this casting hack once refactor is completed
            this.gridBinding.mainGridOptions = <IKendoGridOptions<TDataSource>>(mainGridOptions as any);
        }

        launch() {
            this.$scope.$on("kendoWidgetCreated", (event, widget) => {
                // the event is emitted for every widget; if we have multiple
                // widgets in this controller, we need to check that the event
                // is for the one we're interested in.
                if (widget === this.gridBinding.mainGrid) {

                    this.loadGridOptions();
                    // show loadingbar when export to excel is clicked
                    // hidden again in method exportToExcel callback
                    this.$(".k-grid-excel").click(() => {
                        kendo.ui.progress(this.gridBinding.mainGrid.element, true);
                    });
                }
            });

            //Defer until page change is complete
            this.$timeout(()=> this.build(), 1, false);
        }
    }

    export interface IKendoGridLauncherFactory {
        create<TDataSource>(): IKendoGridLauncher<TDataSource>;
    }

    export class KendoGridLauncherFactory implements IKendoGridLauncherFactory{
        public static $inject: Array<string> = [
            "gridStateService",
            "exportGridToExcelService",
            "notify",
            "$state",
            "$timeout",
            "_",
            "$",
            "$window"
        ];

        constructor(
            private gridStateService: Services.IGridStateFactory,
            private exportGridToExcelService: Services.System.ExportGridToExcelService,
            private notify,
            private $state: ng.ui.IStateService,
            private $timeout: ng.ITimeoutService,
            private _: ILoDashWithMixins,
            private $: JQueryStatic,
            private $window: ng.IWindowService
        ) {

        }

        create<TDataSource>(): IKendoGridLauncher<TDataSource> {
            return new KendoGridLauncher(
                this.gridStateService,
                this.exportGridToExcelService,
                this.notify,
                this.$state,
                this.$timeout,
                this._,
                this.$,
                this.$window);
        }
    }

    app.service("kendoGridLauncherFactory", KendoGridLauncherFactory);

}