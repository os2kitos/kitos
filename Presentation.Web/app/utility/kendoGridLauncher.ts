/**
     * The purpose of this "builder/launcher" module is to encapsulate the logic behind construction of standard KITOS kendo grid
     * in stead of the previous method where "copy-paste" was used as "reuse" strategy.
     *
     * For that reason, if you find a concept, not previously covered by this class, introduce it and use it :-)
     */
module Kitos.Utility.KendoGrid {
    import Helpers = Kitos.Helpers;
    "use strict";

    export enum KendoGridColumnFiltering {
        StartsWith,
        Contains,
        NumberComparision,
        Date,
        FixedValueRange
    }

    export enum KendoGridColumnDataSourceType {
        Date,
        Boolean,
        Number
    }

    export interface IGridViewAccess<TDataSource> {
        mainGrid: IKendoGrid<TDataSource>;
        mainGridOptions: IKendoGridOptions<TDataSource>;
    }

    export interface IExtendedKendoGridColumn<TDataSource> extends IKendoGridColumn<TDataSource> {
        schemaMutation: (map: any) => void;
        kitosIncluded: boolean;
    }

    export interface IKendoParameter {
        textValue: string;
        remoteValue: any;
        optionalContext?: any;
    }

    export enum KendoColumnAlignment {
        Left,
        Right,
        Center
    }

    export type Predicate = () => boolean;

    export interface IKendoGridExcelOnlyColumnBuilder<TDataSource> {
        withId(id: string): IKendoGridExcelOnlyColumnBuilder<TDataSource>;
        withDataSourceName(name: string): IKendoGridExcelOnlyColumnBuilder<TDataSource>;
        withTitle(title: string): IKendoGridExcelOnlyColumnBuilder<TDataSource>;
        withStandardWidth(width: number): IKendoGridExcelOnlyColumnBuilder<TDataSource>;
        withParentColumnId(parentId: string): IKendoGridExcelOnlyColumnBuilder<TDataSource>;
        withExcelOutput(excelOutput: (source: TDataSource) => string): IKendoGridExcelOnlyColumnBuilder<TDataSource>;
        withInclusionCriterion(criterion: () => boolean): IKendoGridExcelOnlyColumnBuilder<TDataSource>;
        build(): IExtendedKendoGridColumn<TDataSource>;
    }

    export interface IKendoGridColumnBuilder<TDataSource> {
        withId(id: string): IKendoGridColumnBuilder<TDataSource>;
        withDataSourceName(name: string): IKendoGridColumnBuilder<TDataSource>;
        withTitle(title: string): IKendoGridColumnBuilder<TDataSource>;
        withStandardWidth(width: number): IKendoGridColumnBuilder<TDataSource>;
        withFilteringOperation(operation: KendoGridColumnFiltering): IKendoGridColumnBuilder<TDataSource>;
        withDataSourceType(dataSourceType: KendoGridColumnDataSourceType): IKendoGridColumnBuilder<TDataSource>;
        withFixedValueRange(possibleValues: IKendoParameter[], multiSelect: boolean, optionalTemplate?: (dataItem: any) => string): IKendoGridColumnBuilder<TDataSource>;
        withoutSorting(): IKendoGridColumnBuilder<TDataSource>;
        withInitialVisibility(visible: boolean): IKendoGridColumnBuilder<TDataSource>;
        withRendering(renderUi: (source: TDataSource) => string): IKendoGridColumnBuilder<TDataSource>;
        withSourceValueEchoRendering(): IKendoGridColumnBuilder<TDataSource>;
        withContentOverflow(): IKendoGridColumnBuilder<TDataSource>;
        withExcelOutput(excelOutput: (source: TDataSource) => string): IKendoGridColumnBuilder<TDataSource>;
        withSourceValueEchoExcelOutput(): IKendoGridColumnBuilder<TDataSource>;
        withContentAlignment(alignment: KendoColumnAlignment): IKendoGridColumnBuilder<TDataSource>;
        withInclusionCriterion(Predicate): IKendoGridColumnBuilder<TDataSource>;
        build(): IExtendedKendoGridColumn<TDataSource>;
    }

    class KendoGridExcelOnlyColumnBuilder<TDataSource> implements IKendoGridExcelOnlyColumnBuilder<TDataSource> {
        private standardWidth: number = 150;
        private title: string = null;
        private id: string = null;
        private dataSourceName: string = null;
        private parentId: string = null;
        private excelOutput: (source: TDataSource) => string = null;
        private inclusionCriterion: Predicate = () => true;

        withInclusionCriterion(criterion: Predicate): IKendoGridExcelOnlyColumnBuilder<TDataSource> {
            if (criterion == null) throw "criterion must be defined";
            this.inclusionCriterion = criterion;
            return this;
        }

        withExcelOutput(excelOutput: (source: TDataSource) => string): IKendoGridExcelOnlyColumnBuilder<TDataSource> {
            if (excelOutput == null) throw "excelOutput must be defined";
            this.excelOutput = excelOutput;
            return this;
        }

        withId(id: string): IKendoGridExcelOnlyColumnBuilder<TDataSource> {
            if (id == null) throw "id must be defined";
            this.id = id;
            return this;
        }

        withDataSourceName(name: string): IKendoGridExcelOnlyColumnBuilder<TDataSource> {
            if (name == null) throw "name must be defined";
            this.dataSourceName = name;
            return this;
        }

        withTitle(title: string): IKendoGridExcelOnlyColumnBuilder<TDataSource> {
            if (title == null) throw "title must be defined";
            this.title = title;
            return this;
        }

        withStandardWidth(width: number): IKendoGridExcelOnlyColumnBuilder<TDataSource> {
            if (width == null) throw "width must be defined";
            this.standardWidth = width;
            return this;
        }

        withParentColumnId(parentId: string): IKendoGridExcelOnlyColumnBuilder<TDataSource> {
            if (parentId == null) throw "parentId must be defined";
            this.parentId = parentId;
            return this;
        }

        private checkRequiredField(name: string, value: any) {
            if (value == null) {
                throw `${name} is a required field and must be provided`;
            }
        }

        build(): IExtendedKendoGridColumn<TDataSource> {
            this.checkRequiredField("title", this.title);
            this.checkRequiredField("id", this.id);
            this.checkRequiredField("dataSourceName", this.dataSourceName);
            this.checkRequiredField("excelOutput", this.excelOutput);

            return {
                field: this.dataSourceName,
                title: this.title,
                attributes: [],
                parentId: this.parentId,
                width: this.standardWidth,
                persistId: `${this.id}_${new Date().getTime()}`, //Make the persistid random every time the grid is built (an error in the export process should not "stick")this.id,
                excelTemplate: this.excelOutput ? (dataItem => this.excelOutput(dataItem)) : null,
                filterable: false,
                sortable: false,
                hidden: true, // Always invisible
                menu: false, //Never selectable in menu or visible in anything but excel sheets
                schemaMutation: () => { },
                kitosIncluded: this.inclusionCriterion()
            } as IExtendedKendoGridColumn<TDataSource>;
        }
    }

    class KendoGridColumnBuilder<TDataSource> implements IKendoGridColumnBuilder<TDataSource> {
        private standardWidth: number = 150;
        private dataSourceName: string = null;
        private title: string = null;
        private filtering: KendoGridColumnFiltering = null;
        private valueRange: IKendoParameter[] = null;
        private valueRangeMultiSelect: boolean = false;
        private valueRangeTemplate: (dataItem: any) => string = null;
        private id: string = null;
        private rendering: (source: TDataSource) => string = null;
        private excelOutput: (source: TDataSource) => string = null;
        private sortingEnabled = true;
        private visible = true;
        private dataSourceType: KendoGridColumnDataSourceType = null;
        private contentOverflow: boolean | null = null;
        private contentAlignment: KendoColumnAlignment | null = null;
        private inclusionCriterion: Predicate = () => true;

        withInclusionCriterion(criterion: Predicate): IKendoGridColumnBuilder<TDataSource> {
            if (criterion == null) throw "criterion must be defined";
            this.inclusionCriterion = criterion;
            return this;
        }

        withContentAlignment(alignment: KendoColumnAlignment): IKendoGridColumnBuilder<TDataSource> {
            this.contentAlignment = alignment;
            return this;
        }

        withContentOverflow(): IKendoGridColumnBuilder<TDataSource> {
            this.contentOverflow = true;
            return this;
        }

        withFixedValueRange(possibleValues: IKendoParameter[], multiSelect: boolean, optionalTemplate?: (dataItem: any) => string): IKendoGridColumnBuilder<TDataSource> {
            if (possibleValues == null) throw "possibleValues must be defined";
            this.valueRange = possibleValues;
            this.valueRangeMultiSelect = multiSelect;
            this.valueRangeTemplate = !!optionalTemplate ? optionalTemplate : null;
            return this;
        }

        withDataSourceType(dataSourceType: KendoGridColumnDataSourceType): IKendoGridColumnBuilder<TDataSource> {
            if (dataSourceType == null) throw "dataSourceType must be defined";
            this.dataSourceType = dataSourceType;
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
            if (this.filtering === KendoGridColumnFiltering.Date) {
                return this.withDataSourceType(KendoGridColumnDataSourceType.Date);
            }
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
            if (this.dataSourceType != null) {
                switch (this.dataSourceType) {
                    case KendoGridColumnDataSourceType.Boolean:
                        return map => map[this.dataSourceName] = { type: "boolean" };
                    case KendoGridColumnDataSourceType.Date:
                        return map => map[this.dataSourceName] = { type: "date" };
                    case KendoGridColumnDataSourceType.Number:
                        return map => map[this.dataSourceName] = { type: "number" };
                    default:
                        throw `Unmapped data source type ${this.dataSourceType}`;
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
                    case KendoGridColumnFiltering.StartsWith:
                        return {
                            cell: {
                                template: (args) =>
                                    args.element.kendoAutoComplete({
                                        noDataTemplate: ""
                                    }),
                                dataSource: [],
                                showOperators: false,
                                operator: "startswith"
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
                    case KendoGridColumnFiltering.NumberComparision:
                        return {
                            operators: {
                                number: {
                                    eq: "Lig med",
                                    neq: "Forskellig fra",
                                    gte: "Større eller lig med",
                                    lte: "Mindre eller lig med"
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
                                            return {
                                                remoteValue: value.remoteValue,
                                                text: value.textValue,
                                                optionalContext: value.optionalContext
                                            };
                                        }),
                                        dataTextField: "text",
                                        dataValueField: "remoteValue",
                                        valuePrimitive: true,
                                        template: this.valueRangeTemplate
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

            const attributes = {
                "data-element-type": `${this.id}KendoObject`
            };

            const classes: string[] = [];
            if (this.contentOverflow) {
                classes.push("might-overflow");
            }
            if (this.contentAlignment != null) {
                switch (this.contentAlignment) {
                    case KendoColumnAlignment.Left:
                        classes.push("text-left");
                        break;
                    case KendoColumnAlignment.Right:
                        classes.push("text-right");
                        break;
                    case KendoColumnAlignment.Center:
                        classes.push("text-center");
                        break;
                    default:
                        throw `Unsupported alignment type:${this.contentAlignment}`;
                }
            }
            if (classes.length > 0) {
                attributes["class"] = classes.join(" ");
            }
            return {
                field: this.dataSourceName,
                title: this.title,
                attributes: attributes,
                width: this.standardWidth,
                hidden: !this.visible,
                persistId: this.id,
                template: dataItem => this.rendering(dataItem),
                excelTemplate: this.excelOutput ? (dataItem => this.excelOutput(dataItem)) : null,
                filterable: this.getFiltering(),
                sortable: this.sortingEnabled,
                schemaMutation: this.getSchemaMutation(),
                kitosIncluded: this.inclusionCriterion()
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

    export enum KendoToolbarImplementation {
        Button,
        Link,
        DropDownList
    }

    export enum KendoToolbarMargin {
        Left,
        Right,
        Down,
        Top
    }

    export interface IKendoToolbarDropDownEntry {
        id: string;
        text: string;
        originalObject?: any;
    }

    export interface IKendoToolbarDropDownConfiguration {
        selectedOptionChanged: (selectedOption: IKendoToolbarDropDownEntry) => void;
        availableOptions: IKendoToolbarDropDownEntry[];
    }

    export interface IKendoToolbarEntry {
        title: string;
        id: string;
        onClick?: () => void;
        link?: string;
        dropDownConfiguration?: IKendoToolbarDropDownConfiguration;
        enabled: () => boolean;
        show?: boolean;
        implementation: KendoToolbarImplementation,
        color: KendoToolbarButtonColor;
        position: KendoToolbarButtonPosition;
        margins?: KendoToolbarMargin[];
    }

    type UrlFactory = (options: any) => string;
    type ResponseParser<TDataSource> = (response: TDataSource[]) => TDataSource[];
    type ColumnConstruction<TDataSource> = (builder: IKendoGridColumnBuilder<TDataSource>) => void;
    type ExcelOnlyColumnConstruction<TDataSource> = (builder: IKendoGridExcelOnlyColumnBuilder<TDataSource>) => void;
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
        withExcelOnlyColumn(build: ExcelOnlyColumnConstruction<TDataSource>): IKendoGridLauncher<TDataSource>;
        withResponseParser(parser: ResponseParser<TDataSource>): IKendoGridLauncher<TDataSource>;
        withParameterMapping(mapping: ParameterMapper): IKendoGridLauncher<TDataSource>;
        withOverviewType(overviewType: Models.Generic.OverviewType): IKendoGridLauncher<TDataSource>;
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
        private columnFactories: (() => IExtendedKendoGridColumn<TDataSource>)[] = [];
        private responseParser: ResponseParser<TDataSource> = response => response;
        private parameterMapper: ParameterMapper = (data, type) => null;
        private overviewType: Models.Generic.OverviewType = null;
        private postCreationActions: Array<(gridBinding: IGridViewAccess<TDataSource>) => void> = [];

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
            this.columnFactories.push(() => {
                const builder = new KendoGridColumnBuilder<TDataSource>();
                build(builder);
                return builder.build();
            });
            return this;
        }

        withExcelOnlyColumn(build: (builder: IKendoGridExcelOnlyColumnBuilder<TDataSource>) => void): IKendoGridLauncher<TDataSource> {
            if (!build) throw "build must be defined";
            this.columnFactories.push(() => {
                const builder = new KendoGridExcelOnlyColumnBuilder<TDataSource>();
                build(builder);
                return builder.build();
            });
            return this;
        }

        withToolbarEntry(entry: IKendoToolbarEntry): IKendoGridLauncher<TDataSource> {
            if (!entry) throw "entry must be defined";
            if (entry.show == null) { //Default to true.
                entry.show = true;
            }
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
            this.gridState = this.gridStateService.getService(this.storageKey, this.user, this.overviewType);
            return this;
        }

        withOverviewType(overviewType: Models.Generic.OverviewType): IKendoGridLauncher<TDataSource> {
            this.overviewType = overviewType;
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

        private refreshData() {
            this.gridBinding.mainGrid.dataSource.read();
        }

        saveGridProfile() {
            this.gridState.saveGridProfile(this.gridBinding.mainGrid);
            this.notify.addSuccessMessage("Filtre og sortering gemt");
        }

        loadGridProfile() {
            this.gridState.loadGridProfile(this.gridBinding.mainGrid);
            this.refreshData();
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

        saveGridForOrganization() {
            if (confirm(`Er du sikker på at du vil gemme nuværende kolonneopsætning af felter som standard til ${this.user.currentOrganizationName}`)) {
                this.gridState.saveGridOrganizationalConfiguration(this.gridBinding.mainGrid, this.overviewType);
            }
        }

        clearGridForOrganization() {
            if (confirm(`Er du sikker på at du vil slette standard kolonneopsætning af felter til ${this.user.currentOrganizationName}`)) {
                this.gridState
                    .deleteGridOrganizationalConfiguration(this.overviewType)
                    .then(() => this.reload());
            }
        }

        showGridForOrganizationButtons() {
            if (this.overviewType !== null) {
                return this.user.isLocalAdmin;
            }
            return false;
        }

        canDeleteGridForOrganization() {
            return this.gridState.canDeleteGridOrganizationalConfiguration();
        }

        doesGridDivergeFromDefault() {
            return this.gridState.doesGridDivergeFromOrganizationalConfiguration(this.overviewType, this.gridBinding.mainGrid);
        }

        // clears grid filters by removing the localStorageItem and reloading the page
        clearOptions() {
            this.gridState.reset();
            this.notify.addSuccessMessage("Sortering, filtering og kolonnevisning, -bredde og –rækkefølge gendannet til standardopsætning ");
            // have to reload entire page, as dataSource.read() + grid.refresh() doesn't work :(
            this.reload();
        }

        private reload() {
            this.$state.go(".", null, { reload: true });
        }

        private excelConfig: Models.IExcelConfig = {
        };

        private exportToExcel = (e: IKendoGridExcelExportEvent<TDataSource>) => {
            this.exportGridToExcelService.getExcel(e, this._, this.$timeout, this.gridBinding.mainGrid, this.excelConfig);
        }

        private checkRequiredField(name: string, value: any) {
            if (value == null) {
                throw `${name} is a required field and must be provided`;
            }
        }

        private applyDeferredActions() {
            this.postCreationActions.forEach(action => action(this.gridBinding));
            this.postCreationActions = []; //Clear any bindings held in this array
        }

        private build() {
            this.checkRequiredField("$scope", this.$scope);
            this.checkRequiredField("storageKey", this.storageKey);
            this.checkRequiredField("entityTypeName", this.entityTypeName);
            this.checkRequiredField("urlFactory", this.urlFactory);
            this.checkRequiredField("standardSortingSourceField", this.standardSortingSourceField);
            this.checkRequiredField("gridBinding", this.gridBinding);

            this.$scope.kendoVm = {
                standardToolbar: {
                    //NOTE: Intentional wrapping of the functions to capture the "this" reference and hereby the state (this will otherwise be null inside the function calls)
                    clearOptions: () => this.clearOptions(),
                    saveGridProfile: () => this.saveGridProfile(),
                    doesGridProfileExist: () => this.doesGridProfileExist(),
                    saveGridForOrganization: () => this.saveGridForOrganization(),
                    clearGridForOrganization: () => this.clearGridForOrganization(),
                    showGridForOrganizationButtons: () => this.showGridForOrganizationButtons(),
                    canDeleteGridForOrganization: () => this.canDeleteGridForOrganization(),
                    doesGridDivergeFromDefault: () => this.doesGridDivergeFromDefault(),
                    gridDivergenceText: () => this.doesGridDivergeFromDefault() ? "OBS: Opsætning af overblik afviger fra kommunens standardoverblik. Tryk på 'Gendan kolonneopsætning' for at benytte den gældende opsætning" : ""
                }
            };

            var toolbar = [
                {
                    name: "clearFilter",
                    text: "Gendan kolonneopsætning",
                    template:
                        "<button data-element-type='resetFilterButton' type='button' class='k-button k-button-icontext' title='{{kendoVm.standardToolbar.gridDivergenceText()}}' data-ng-click='kendoVm.standardToolbar.clearOptions()'>#: text # <i class='fa fa-exclamation-circle warning-icon-right-of-text' ng-if='kendoVm.standardToolbar.doesGridDivergeFromDefault()'></i></button>"
                },
                {
                    name: "saveFilter",
                    text: "Gem filter",
                    template:
                        '<button data-element-type="saveFilterButton" type="button" class="k-button k-button-icontext" title="Gem filtre og sortering" data-ng-click="kendoVm.standardToolbar.saveGridProfile()">#: text #</button>'
                },
                {
                    name: "useFilter",
                    text: "Anvend filter",
                    template:
                        '<button data-element-type="useFilterButton" type="button" class="k-button k-button-icontext" title="Anvend gemte filtre og sortering" data-ng-click="kendoVm.standardToolbar.loadGridProfile()" data-ng-disabled="!kendoVm.standardToolbar.doesGridProfileExist()">#: text #</button>'
                },
                {
                    name: "deleteFilter",
                    text: "Slet filter",
                    template:
                        "<button data-element-type='removeFilterButton' type='button' class='k-button k-button-icontext' title='Slet filtre og sortering' data-ng-click='kendoVm.standardToolbar.clearGridProfile()' data-ng-disabled='!kendoVm.standardToolbar.doesGridProfileExist()'>#: text #</button>"
                },
                {
                    name: "filterOrg",
                    text: "Gem kolonneopsætning for organisation",
                    template: "<button data-element-type='filterOrgButton' type='button' class='k-button k-button-icontext' title='Gem kolonneopsætning for organisation' data-ng-click='kendoVm.standardToolbar.saveGridForOrganization()' ng-show='kendoVm.standardToolbar.showGridForOrganizationButtons()'>#: text #</button>"
                },
                {
                    name: "removeFilterOrg",
                    text: "Slet kolonneopsætning for organisation",
                    template: "<button data-element-type='removeFilterOrgButton' type='button' class='k-button k-button-icontext' title='Slet kolonneopsætning for organisation' data-ng-click='kendoVm.standardToolbar.clearGridForOrganization()' data-ng-disabled='!kendoVm.standardToolbar.canDeleteGridForOrganization()' ng-show='kendoVm.standardToolbar.showGridForOrganizationButtons()'>#: text #</button>"
                }
            ];

            //Add the excel export button with multiple options
            const excelExportDropdownEntry = Helpers.ExcelExportHelper.createExcelExportDropdownEntry(() => this.excelConfig, () => this.gridBinding.mainGrid);
            this.customToolbarEntries.push(excelExportDropdownEntry);

            this._.forEach(this.customToolbarEntries, entry => {
                switch (entry.implementation) {
                    case KendoToolbarImplementation.Button:
                        toolbar.push({
                            name: entry.id,
                            text: entry.title,
                            template: `<button data-element-type='${entry.id}Button' type='button' class='${Helpers.KendoToolbarCustomizationHelper.getColorClass(entry.color)} ${Helpers.KendoToolbarCustomizationHelper.getPositionClass(entry.position)} ${Helpers.KendoToolbarCustomizationHelper.getMargins(entry.margins)}' title='${entry.title}' data-ng-click='kendoVm.${entry.id}.onClick()' data-ng-disabled='!kendoVm.${entry.id}.enabled' ng-show='kendoVm.${entry.id}.show'>#: text #</button>`
                        });
                        this.$scope.kendoVm[entry.id] = {
                            onClick: entry.onClick,
                            enabled: entry.enabled(),
                            show: entry.show
                        };
                        break;
                    case KendoToolbarImplementation.Link:
                        toolbar.push({
                            name: entry.id,
                            text: entry.title,
                            template: `<a data-element-type='${entry.id}Button' role='button' class='${Helpers.KendoToolbarCustomizationHelper.getColorClass(entry.color)} ${Helpers.KendoToolbarCustomizationHelper.getPositionClass(entry.position)}' id='gdprExportAnchor' href='${entry.link}' data-ng-disabled='!kendoVm.${entry.id}.enabled'>#: text #</a>`
                        });
                        this.$scope.kendoVm[entry.id] = {
                            enabled: entry.enabled()
                        };
                        break;
                    case KendoToolbarImplementation.DropDownList:
                        toolbar.push({
                            name: entry.id,
                            text: entry.title,
                            template: `<select data-element-type='${entry.id}DropDownList' id='${entry.id}' class='${Helpers.KendoToolbarCustomizationHelper.getPositionClass(entry.position)} ${Helpers.KendoToolbarCustomizationHelper.getMargins(entry.margins)}' kendo-drop-down-list="kendoVm.${entry.id}.list" k-options="kendoVm.${entry.id}.getOptions()"></select>`
                        });
                        this.$scope.kendoVm[entry.id] = {
                            enabled: entry.enabled(),
                            getOptions: () => {
                                // The excel options are customized and not a generic dropdown
                                if (entry === excelExportDropdownEntry) {
                                    return Helpers.ExcelExportHelper.createExportToExcelDropDownOptions(entry);
                                }
                                return {
                                    autoBind: false,
                                    dataSource: entry.dropDownConfiguration.availableOptions,
                                    dataTextField: "text",
                                    dataValueField: "id",
                                    optionLabel: entry.title,
                                    change: e => {
                                        var selectedId = e.sender.value();
                                        const newSelection = entry.dropDownConfiguration.availableOptions.filter(x => x.id === selectedId);
                                        entry.dropDownConfiguration.selectedOptionChanged(newSelection.length > 0 ? newSelection[0] : null);
                                        this.saveGridOptions();
                                    }
                                }
                            }
                        };
                        break;
                    default:
                        throw `Invalid toolbar implementation type:${entry.implementation}`;
                }
            });

            //Build the columns
            var columns = [];
            var schemaFields = {};
            this._.forEach(this.columnFactories,
                factory => {
                    const gridColumn = factory();
                    if (gridColumn.kitosIncluded) {
                        gridColumn.schemaMutation(schemaFields);
                        columns.push(gridColumn);
                    }
                });

            //Build the grid
            const defaultPageSize = 100;
            const validPageSizes = [10, 25, 50, 100, 200, "all"];
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
                    pageSize: defaultPageSize,
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
                    pageSizes: validPageSizes,
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

            this.gridState
                .applySavedGridOptions(mainGridOptions)
                .then((settingsToBeLoadedAfterRendering) => {
                    //Saved indexes must be applied after rendering since the map only contains values for visible columns. sorting beforehand will move all currently invisible columns out of the original order, and that will affect the filter menu.
                    this.postCreationActions.push(access => {
                        const createdColumns = access.mainGrid.columns;
                        settingsToBeLoadedAfterRendering.columnOrder.forEach(savedColumnOrder => {
                            var columnIndex = this._.findIndex(createdColumns, column => {
                                if (!column.hasOwnProperty("persistId")) {
                                    console.error(`Unable to find persistId property in grid column with field=${column.field}`);
                                    return false;
                                }

                                return column.persistId === savedColumnOrder.persistId;
                            });
                            if (columnIndex !== -1) {
                                var columnObj = createdColumns[columnIndex];
                                // reorder column
                                if (savedColumnOrder.columnIndex !== columnIndex) {
                                    // check if index is out of bounds
                                    if (savedColumnOrder.columnIndex < createdColumns.length) {
                                        access.mainGrid.reorderColumn(savedColumnOrder.columnIndex, columnObj);
                                    }
                                }
                            }
                        });
                    });

                    this.postCreationActions.push(_ => {
                        //Loading the data from the server now that the grid is ready
                        //NOTE: Using the pageSize() method since read().. will change the pagesize to the size of the response.. this way we keep inside the stored ranges
                        let pageSize = settingsToBeLoadedAfterRendering.pageSize;
                        if (!pageSize || validPageSizes.indexOf(pageSize) === -1) {
                            pageSize = defaultPageSize;
                        }
                        this.gridBinding.mainGrid.dataSource.pageSize(pageSize);
                    });
                    // assign the generated grid options. Kendo will start after this
                    this.gridBinding.mainGridOptions = mainGridOptions;
                });
        }

        launch() {
            let awaitingDeferredActions = true;

            this.$scope.$on("kendoWidgetCreated", (_, widget) => {
                // the event is emitted for every widget; if we have multiple
                // widgets in this controller, we need to check that the event
                // is for the one we're interested in.
                if (widget === this.gridBinding.mainGrid) {
                    if (awaitingDeferredActions) {
                        this.applyDeferredActions();
                        awaitingDeferredActions = false;
                    }
                    // show loadingbar when export to excel is clicked
                    // hidden again in method exportToExcel callback
                    this.$(".k-grid-excel").click(() => {
                        kendo.ui.progress(this.gridBinding.mainGrid.element, true);
                    });
                }
            });
            this.build();
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