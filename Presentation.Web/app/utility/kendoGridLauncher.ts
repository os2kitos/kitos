module Kitos.Utility.KendoGrid {
    "use strict";
    import IDataProcessingAgreement = Models.DataProcessing.IDataProcessingAgreement;

    export interface IGridViewAccess<TDataSource> {
        mainGrid: IKendoGrid<TDataSource>;
        mainGridOptions: IKendoGridOptions<TDataSource>;
    }

    export enum KendoToolbarButtonColor {
        Grey,
        Green
    }

    export enum KendoToolbarButtonPosition {
        Left,
        Right
    }

    export interface IKendoToolbarEntry {
        title: string;
        id: string;
        onClick: () => void;
        enabled: () => boolean;
        color: KendoToolbarButtonColor;
        position: KendoToolbarButtonPosition;
    }

    type UrlFactory = (options: any) => string;

    export interface IKendoGridLauncher<TDataSource> {
        launch(): void;
        withUser(user: Services.IUser): IKendoGridLauncher<TDataSource>;
        withGridBinding(gridBinding: IGridViewAccess<TDataSource>): IKendoGridLauncher<TDataSource>;
        withEntityTypeName(name: string): IKendoGridLauncher<TDataSource>;
        withExcelOutputName(name: string): IKendoGridLauncher<TDataSource>;
        withStorageKey(newKey: string): IKendoGridLauncher<TDataSource>;
        withScope($scope: ng.IScope): IKendoGridLauncher<TDataSource>;
        withFixedSourceUrl(url: string): IKendoGridLauncher<TDataSource>;
        withUrlFactory(factory: UrlFactory): IKendoGridLauncher<TDataSource>;
        withToolbarEntry(entry: IKendoToolbarEntry): IKendoGridLauncher<TDataSource>;
    }

    /**
     * The purpose of this "builder/launcher" is to encapsulate the logic behind construction of standard KITOS kendo grid
     * in stead of the previous method where "copy-paste" was used as "reuse" strategy.
     *
     * For that reason, if you find a concept, not previously covered by this class, introduce it and use it :-)
     */
    export class KendoGridLauncher<TDataSource> implements IKendoGridLauncher<TDataSource>{
        private $scope: ng.IScope = null;
        private storageKey: string = null;
        private gridState: Services.IGridStateService = null;
        private entityTypeName: string = null;
        private excelOutputName: string = null;
        private gridBinding: IGridViewAccess<TDataSource> = null;
        private user: Services.IUser = null;
        private urlFactory: UrlFactory = null;
        private customToolbarEntries: IKendoToolbarEntry[] = [];

        constructor(
            private readonly gridStateService: Services.IGridStateFactory,
            private readonly exportGridToExcelService: Services.System.ExportGridToExcelService,
            private readonly notify,
            private readonly $state: ng.ui.IStateService,
            private readonly $timeout: ng.ITimeoutService,
            private readonly _: ILoDashWithMixins,
            private readonly $: JQueryStatic,
            private readonly $window: ng.IWindowService
        ) {

        }

        withToolbarEntry(entry: IKendoToolbarEntry): IKendoGridLauncher<TDataSource> {
            if (!entry) throw "entry must be defined";
            this.customToolbarEntries.push(entry);
            return this;
        }

        withUrlFactory(factory: UrlFactory): IKendoGridLauncher<TDataSource> {
            if (!factory) throw "factory must be defined";
            this.urlFactory = factory;
            return this;
        }

        withFixedSourceUrl(url: string): IKendoGridLauncher<TDataSource> {
            if (!url) throw "url must be defined";
            return this.withUrlFactory(_ => url);
        }

        withScope($scope: ng.IScope): IKendoGridLauncher<TDataSource> {
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
            if (!gridBinding) throw "gridBinding must be defined";
            this.gridBinding = gridBinding;
            return this;
        }

        withEntityTypeName(name: string): IKendoGridLauncher<TDataSource> {
            if (!name) throw "name must be defined";
            this.entityTypeName = name;
            return this;
        }

        withExcelOutputName(name: string): IKendoGridLauncher<TDataSource> {
            if (!name) throw "name must be defined";
            this.excelOutputName = name;
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
      
        // saves grid state to localStorage
        private saveGridOptions = () => {
            this.gridState.saveGridOptions(this.gridBinding.mainGrid);
        }

        // Resets the scrollbar position
        private onPaging = () => {
            KendoGrid.KendoGridScrollbarHelper.resetScrollbarPosition(this.gridBinding.mainGrid);
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
            return this.gridState.doesGridProfileExist();
        }

        // clears grid filters by removing the localStorageItem and reloading the page
        clearOptions() {
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

        private exportToExcel = (e: IKendoGridExcelExportEvent<TDataSource>) => {
            this.exportGridToExcelService.getExcel(e, this._, this.$timeout, this.gridBinding.mainGrid);
        }

        private build() {
            //TODO: Check required fields
            //TODO: Refactor content of this to use TDataSource in stead and column creation as well + url

            //Build toolbar buttons
            var getColorClass = (color: KendoToolbarButtonColor): string => {
                switch (color) {
                    case KendoToolbarButtonColor.Green:
                        return "btn btn-success";
                    case KendoToolbarButtonColor.Grey:
                        return "k-button k-button-icontext";
                    default:
                        throw `Unknown color ${color}`;
                }
            };

            var getPositionClass = (position: KendoToolbarButtonPosition): string => {
                switch (position) {
                    case KendoToolbarButtonPosition.Left:
                        return "";
                    case KendoToolbarButtonPosition.Right:
                        return "pull-right";
                    default:
                        throw `Unknown position ${position}`;
                }
            };

            this.$scope.kendoVm = {
                standardToolbar: {
                    //NOTE: Intentional wrapping of the functions to capture the "this" reference and hereby the state (this will otherwise be null inside the function calls)
                    clearOptions: () => this.clearOptions(),
                    saveGridProfile: () => this.saveGridProfile(),
                    loadGridProfile: () => this.loadGridProfile(),
                    clearGridProfile: () => this.clearGridProfile(),
                    doesGridProfileExist: () => this.doesGridProfileExist(),
                }
            };

            var toolbar = [
                {
                    name: "clearFilter",
                    text: "Nulstil",
                    template:
                        "<button type='button' class='k-button k-button-icontext' title='Nulstil sortering, filtering og kolonnevisning, -bredde og –rækkefølge' data-ng-click='kendoVm.standardToolbar.clearOptions()'>#: text #</button>"
                },
                {
                    name: "saveFilter",
                    text: "Gem filter",
                    template:
                        '<button type="button" class="k-button k-button-icontext" title="Gem filtre og sortering" data-ng-click="kendoVm.standardToolbar.saveGridProfile()">#: text #</button>'
                },
                {
                    name: "useFilter",
                    text: "Anvend filter",
                    template:
                        '<button type="button" class="k-button k-button-icontext" title="Anvend gemte filtre og sortering" data-ng-click="kendoVm.standardToolbar.loadGridProfile()" data-ng-disabled="!kendoVm.standardToolbar.doesGridProfileExist()">#: text #</button>'
                },
                {
                    name: "deleteFilter",
                    text: "Slet filter",
                    template:
                        "<button type='button' class='k-button k-button-icontext' title='Slet filtre og sortering' data-ng-click='kendoVm.standardToolbar.clearGridProfile()' data-ng-disabled='!kendoVm.standardToolbar.doesGridProfileExist()'>#: text #</button>"
                }
            ];

            _.forEach(this.customToolbarEntries, entry => {
                toolbar.push({
                    name: entry.id,
                    text: entry.title,
                    template: `<button data-element-type='${entry.id}Button' type='button' class='${getColorClass(entry.color)} ${getPositionClass(entry.position)}' title='${entry.title}' data-ng-click='kendoVm.${entry.id}.onClick()' data-ng-disabled='!kendoVm.${entry.id}.enabled'>#: text #</button>`
                });
                this.$scope.kendoVm[entry.id] = {
                    onClick: entry.onClick,
                    enabled: entry.enabled()
                };
            });

            //Build the grid
            const mainGridOptions: IKendoGridOptions<IDataProcessingAgreement> = {
                autoBind: false, // disable auto fetch, it's done in the kendoRendered event handler
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: options => this.urlFactory(options),
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
                toolbar: toolbar,
                excel: {
                    fileName: this.excelOutputName ? `${this.excelOutputName}.xlsx` : `${this.entityTypeName}  Overblik.xlsx`,
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
            this.$timeout(() => this.build(), 1, false);
        }
    }

    export interface IKendoGridLauncherFactory {
        create<TDataSource>(): IKendoGridLauncher<TDataSource>;
    }

    export class KendoGridLauncherFactory implements IKendoGridLauncherFactory {
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
            private readonly gridStateService: Services.IGridStateFactory,
            private readonly exportGridToExcelService: Services.System.ExportGridToExcelService,
            private readonly notify,
            private readonly $state: ng.ui.IStateService,
            private readonly $timeout: ng.ITimeoutService,
            private readonly _: ILoDashWithMixins,
            private readonly $: JQueryStatic,
            private readonly $window: ng.IWindowService
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