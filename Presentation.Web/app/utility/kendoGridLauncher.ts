/**
     * The purpose of this "builder/launcher" module is to encapsulate the logic behind construction of standard KITOS kendo grid
     * in stead of the previous method where "copy-paste" was used as "reuse" strategy.
     *
     * For that reason, if you find a concept, not previously covered by this class, introduce it and use it :-)
     */
module Kitos.Utility.KendoGrid {
    "use strict";

    export enum KendoGridColumnFiltering {
        Contains,
        Date,
        FixedValueRange
    }

    export interface IGridViewAccess<TDataSource> {
        mainGrid: IKendoGrid<TDataSource>;
        mainGridOptions: IKendoGridOptions<TDataSource>;
    }

    export interface IExtendedKendoGridColumn<TDataSource> extends IKendoGridColumn<TDataSource> {
        schemaMutation: (map: any) => void;
    }

    export interface IKendoGridColumnBuilder<TDataSource> {
        withId(id: string): IKendoGridColumnBuilder<TDataSource>;
        withDataSourceName(name: string): IKendoGridColumnBuilder<TDataSource>;
        withTitle(title: string): IKendoGridColumnBuilder<TDataSource>;
        withStandardWidth(width: number): IKendoGridColumnBuilder<TDataSource>;
        withFilteringOperation(operation: KendoGridColumnFiltering): IKendoGridColumnBuilder<TDataSource>;
        withFixedValueRange(possibleValues: string[], multiSelect : boolean): IKendoGridColumnBuilder<TDataSource>;
        withoutSorting(): IKendoGridColumnBuilder<TDataSource>;
        withInitialVisibility(visible: boolean): IKendoGridColumnBuilder<TDataSource>;
        withRendering(renderUi: (source: TDataSource) => string): IKendoGridColumnBuilder<TDataSource>;
        withSourceValueEchoRendering(): IKendoGridColumnBuilder<TDataSource>;
        withExcelOutput(excelOutput: (source: TDataSource) => string): IKendoGridColumnBuilder<TDataSource>;
        withSourceValueEchoExcelOutput(): IKendoGridColumnBuilder<TDataSource>;
        build(): IExtendedKendoGridColumn<TDataSource>;
    }

    class KendoGridColumnBuilder<TDataSource> implements IKendoGridColumnBuilder<TDataSource> {
        private standardWidth: number = 200;
        private dataSourceName: string = null;
        private title: string = null;
        private filtering: KendoGridColumnFiltering = null;
        private valueRange: string[] = null;
        private valueRangeMultiSelect: boolean = false;
        private id: string = null;
        private rendering: (source: TDataSource) => string = null;
        private excelOutput: (source: TDataSource) => string = null;
        private sortingEnabled = true;
        private visible = true;

        withFixedValueRange(possibleValues: string[], multiSelect : boolean): IKendoGridColumnBuilder<TDataSource> {
            if (possibleValues == null) throw "possibleValues must be defined";
            this.valueRange = possibleValues;
            this.valueRangeMultiSelect = multiSelect;
            return this;
        }

        withInitialVisibility(visible: boolean): IKendoGridColumnBuilder<TDataSource> {
            this.visible = visible;
            return this;
        }

        withoutSorting(): IKendoGridColumnBuilder<TDataSource> {
            this.sortingEnabled = false;
            return this;
        }

        withRendering(renderUi: (source: TDataSource) => string): IKendoGridColumnBuilder<TDataSource> {
            if (renderUi == null) throw "renderUi must be defined";
            this.rendering = renderUi;
            return this;
        }

        private echoSourceValue(dataSource: TDataSource): string {
            const dynamicSource = dataSource as any;
            if (dynamicSource && dynamicSource[this.dataSourceName]) {
                return dynamicSource[this.dataSourceName];
            }
            return "";
        }

        withSourceValueEchoRendering(): IKendoGridColumnBuilder<TDataSource> {
            return this.withRendering(dataSource => this.echoSourceValue(dataSource));
        }

        withSourceValueEchoExcelOutput(): IKendoGridColumnBuilder<TDataSource> {
            return this.withExcelOutput(dataSource => this.echoSourceValue(dataSource));
        }

        withExcelOutput(excelOutput: (source: TDataSource) => string): IKendoGridColumnBuilder<TDataSource> {
            if (excelOutput == null) throw "excelOutput must be defined";
            this.excelOutput = excelOutput;
            return this;
        }

        withFilteringOperation(operation: KendoGridColumnFiltering): IKendoGridColumnBuilder<TDataSource> {
            if (operation == null) throw "operation must be defined";
            this.filtering = operation;
            return this;
        }

        withId(id: string): IKendoGridColumnBuilder<TDataSource> {
            if (id == null) throw "id must be defined";
            this.id = id;
            return this;
        }

        withDataSourceName(name: string): IKendoGridColumnBuilder<TDataSource> {
            if (name == null) throw "name must be defined";
            this.dataSourceName = name;
            return this;
        }

        withTitle(title: string): IKendoGridColumnBuilder<TDataSource> {
            if (title == null) throw "title must be defined";
            this.title = title;
            return this;
        }

        withStandardWidth(width: number): IKendoGridColumnBuilder<TDataSource> {
            if (width == null) throw "width must be defined";
            this.standardWidth = width;
            return this;
        }

        private checkRequiredField(name: string, value: any) {
            if (value == null) {
                throw `${name} is a required field and must be provided`;
            }
        }

        private getSchemaMutation(): (map: any) => void {
            if (this.filtering != null) {
                switch (this.filtering) {
                    case KendoGridColumnFiltering.Date:
                        return map => map[this.dataSourceName] = { type: "date" };
                    default:
                        break;
                }
            }
            return _ => { }; //NOP
        }

        private getFiltering(): boolean | kendo.ui.GridColumnFilterable {
            if (this.filtering != null) {
                switch (this.filtering) {
                    case KendoGridColumnFiltering.Contains:
                        return {
                            cell: {
                                template: (args) =>
                                    args.element.kendoAutoComplete({
                                        noDataTemplate: ""
                                    }),
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        } as any as kendo.ui.GridColumnFilterable;
                    case KendoGridColumnFiltering.Date:
                        return {
                            operators: {
                                date: {
                                    eq: "Lig med",
                                    gte: "Fra og med",
                                    lte: "Til og med"
                                }
                            }
                        } as any as kendo.ui.GridColumnFilterable;
                    case KendoGridColumnFiltering.FixedValueRange:
                        if (this.valueRange === null) {
                            throw new Error(
                                "this.valueRange must be defined when using filtering option FixedValueRange");
                        }
                        const valueRange = this.valueRange; //capture the reference to use in lambda below
                        return {
                            cell: {
                                template: (args) => {
                                    args.element.kendoDropDownList({
                                        dataSource: valueRange.map(value => {
                                            return { textValue: value, text: value };
                                        } ),
                                        dataTextField: "text",
                                        dataValueField: "textValue",
                                        valuePrimitive: true
                                    });
                                },
                                showOperators: false,
                                operator: this.valueRangeMultiSelect ? "contains" : "eq"
                            }
                        } as any as kendo.ui.GridColumnFilterable;
                    default:
                        throw `Unknown filtering strategy ${this.filtering}`;
                }
            }
            return false;
        }

        build(): IExtendedKendoGridColumn<TDataSource> {
            this.checkRequiredField("dataSourceName", this.dataSourceName);
            this.checkRequiredField("title", this.title);
            this.checkRequiredField("id", this.id);
            this.checkRequiredField("rendering", this.rendering);

            return {
                field: this.dataSourceName,
                title: this.title,
                attributes: {
                    "data-element-type": `${this.id}KendoObject`
                },
                width: this.standardWidth,
                hidden: !this.visible,
                persistId: this.id,
                template: dataItem => this.rendering(dataItem),
                excelTemplate: this.excelOutput ? (dataItem => this.excelOutput(dataItem)) : null,
                filterable: this.getFiltering(),
                sortable: this.sortingEnabled,
                schemaMutation: this.getSchemaMutation()
            } as IExtendedKendoGridColumn<TDataSource>;
        }
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
    type ResponseParser<TDataSource> = (response: TDataSource[]) => TDataSource[];
    type ColumnConstruction<TDataSource> = (builder: IKendoGridColumnBuilder<TDataSource>) => void;
    type ParameterMapper = (data: kendo.data.DataSourceTransportParameterMapData, type: string) => any;

    export interface IKendoGridLauncher<TDataSource> {
        launch(): void;
        withUser(user: Services.IUser): IKendoGridLauncher<TDataSource>;
        withGridBinding(gridBinding: IGridViewAccess<TDataSource>): IKendoGridLauncher<TDataSource>;
        withStandardSorting(sourceField: string): IKendoGridLauncher<TDataSource>;
        withEntityTypeName(name: string): IKendoGridLauncher<TDataSource>;
        withExcelOutputName(name: string): IKendoGridLauncher<TDataSource>;
        withStorageKey(newKey: string): IKendoGridLauncher<TDataSource>;
        withScope($scope: ng.IScope): IKendoGridLauncher<TDataSource>;
        withFixedSourceUrl(url: string): IKendoGridLauncher<TDataSource>;
        withUrlFactory(factory: UrlFactory): IKendoGridLauncher<TDataSource>;
        withToolbarEntry(entry: IKendoToolbarEntry): IKendoGridLauncher<TDataSource>;
        withColumn(build: ColumnConstruction<TDataSource>): IKendoGridLauncher<TDataSource>;
        withResponseParser(parser: ResponseParser<TDataSource>): IKendoGridLauncher<TDataSource>;
        withParameterMapping(mapping: ParameterMapper): IKendoGridLauncher<TDataSource>;
    }

    export class KendoGridLauncher<TDataSource> implements IKendoGridLauncher<TDataSource>{
        private $scope: ng.IScope = null;
        private standardSortingSourceField: string = null;
        private storageKey: string = null;
        private gridState: Services.IGridStateService = null;
        private entityTypeName: string = null;
        private excelOutputName: string = null;
        private gridBinding: IGridViewAccess<TDataSource> = null;
        private user: Services.IUser = null;
        private urlFactory: UrlFactory = null;
        private customToolbarEntries: IKendoToolbarEntry[] = [];
        private columns: ColumnConstruction<TDataSource>[] = [];
        private responseParser: ResponseParser<TDataSource> = response => response;
        private parameterMapper: ParameterMapper = (data, type) => null;

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

        withParameterMapping(mapping: ParameterMapper): IKendoGridLauncher<TDataSource> {
            if (!mapping) throw "mapping must be defined";
            this.parameterMapper = mapping;
            return this;
        }

        withResponseParser(parser: ResponseParser<TDataSource>): IKendoGridLauncher<TDataSource> {
            if (!parser) throw "parser must be defined";
            this.responseParser = parser;
            return this;
        }

        withStandardSorting(sourceField: string): IKendoGridLauncher<TDataSource> {
            if (!sourceField) throw "sourceField must be defined";
            this.standardSortingSourceField = sourceField;
            return this;
        }

        withColumn(build: ColumnConstruction<TDataSource>): IKendoGridLauncher<TDataSource> {
            if (!build) throw "build must be defined";
            this.columns.push(build);
            return this;
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

        private checkRequiredField(name: string, value: any) {
            if (value == null) {
                throw `${name} is a required field and must be provided`;
            }
        }

        private build() {
            this.checkRequiredField("$scope", this.$scope);
            this.checkRequiredField("storageKey", this.storageKey);
            this.checkRequiredField("entityTypeName", this.entityTypeName);
            this.checkRequiredField("urlFactory", this.urlFactory);
            this.checkRequiredField("standardSortingSourceField", this.standardSortingSourceField);
            this.checkRequiredField("gridBinding", this.gridBinding);

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

            this._.forEach(this.customToolbarEntries, entry => {
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

            //Build the columns
            var columns = [];
            var schemaFields = {};
            this._.forEach(this.columns,
                build => {
                    const builder = new KendoGridColumnBuilder<TDataSource>();
                    build(builder);
                    const gridColumn = builder.build();
                    gridColumn.schemaMutation(schemaFields);
                    columns.push(gridColumn);
                });

            //Build the grid
            const mainGridOptions: IKendoGridOptions<TDataSource> = {
                autoBind: false, // disable auto fetch, it's done in the kendoRendered event handler
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: options => this.urlFactory(options),
                            dataType: "json"
                        },
                        parameterMap: (data: kendo.data.DataSourceTransportParameterMapData, type: string) => this.parameterMapper(data, type)
                    },
                    sort: {
                        field: this.standardSortingSourceField,
                        dir: "asc"
                    },
                    pageSize: 100,
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true,
                    schema: {
                        model: {
                            fields: schemaFields,
                        },
                        parse: response => {
                            response.value = this.responseParser(response.value);
                            return response;
                        }
                    }
                },
                toolbar: toolbar,
                excel: {
                    fileName: this.excelOutputName
                        ? `${this.excelOutputName}.xlsx`
                        : `${this.entityTypeName}  Overblik.xlsx`,
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
                columns: columns,
            };

            // assign the generated grid options to the scope value, kendo will do the rest
            this.gridBinding.mainGridOptions = mainGridOptions;
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
        static $inject: Array<string> = [
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