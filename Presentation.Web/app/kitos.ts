module Kitos {
    export interface IRootScope extends ng.IRootScopeService {
        page: { title: string};
    }

    export interface IKendoGridColumn<TDataSource> extends kendo.ui.GridColumn {
        persistId: string;
        tempVisual?: boolean;
        excelTemplate?(dataItem: TDataSource): string;
        template?: ((dataItem: TDataSource) => string)|string;
    }

    export interface IKendoGridToolbarItem extends kendo.ui.GridToolbarItem {
        className?: string;
    }

    export interface IKendoGridOptions<TDataSource> extends kendo.ui.GridOptions {
        toolbar?: IKendoGridToolbarItem[];
        columns?: IKendoGridColumn<TDataSource>[];
        excelOnlyColumns?: Kitos.Services.System.IKendoGridAdditionalExcelColumn[];
        detailTemplate?: ((dataItem: TDataSource) => string)|string;
    }

    export interface IKendoGrid<TDataSource> extends kendo.ui.Grid {
        getOptions(): IKendoGridOptions<TDataSource>;
        columns: IKendoGridColumn<TDataSource>[];
    }

    export interface IKendoDataObservableObject extends kendo.data.ObservableObject {
        id: number;
    }

    export interface ILoDashWithMixins extends _.LoDashStatic {
        deep(obj, key, value?);
        pluckDeep(obj, key);
        unpick(obj);
        resursivePluck(obj, key, childPropertyName?);
        addHierarchyLevelOnNested(objAry, level?, childPropertyName?);
        addHierarchyLevelOnFlatAndSort(objAry, idPropertyName?, parentIdPropertyName?);
        removeFiltersForField(filterObj, field);
        addFilter(filterObj, field, operator, value, logic);
        findKeyDeep(obj, keyObj);
    }

    export interface IKendoGridExcelExportEvent<TDataSource> extends kendo.ui.GridExcelExportEvent {
        sender: IKendoGrid<TDataSource>;
    }

    export interface IRequestShortcutConfigWithCustomConfig extends ng.IRequestShortcutConfig {
        [key: string]: any;
    }

    export interface IHttpServiceWithCustomConfig extends ng.IHttpService {
        post<T>(url: string, data: any, config?: IRequestShortcutConfigWithCustomConfig): ng.IHttpPromise<T>;
    }

    export interface IRequestConfigWithCustomConfig extends ng.IRequestConfig, IRequestShortcutConfigWithCustomConfig {

    }

    export interface IHttpInterceptorWithCustomConfig extends ng.IHttpInterceptor {
        request?: (config: IRequestConfigWithCustomConfig) => IRequestConfigWithCustomConfig | ng.IPromise<IRequestConfigWithCustomConfig>;
    }

    export interface AuthRoles extends ng.ui.IStateProvider {
        authRoles: [Models.OrganizationRole|"GlobalAdmin"];
        noAuth: string;
        name: string;
    }
}
