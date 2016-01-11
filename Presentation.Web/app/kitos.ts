module Kitos {
    export interface IRootScope extends ng.IRootScopeService {
        page: { title: string};
    }

    export interface IKendoGridColumn extends kendo.ui.GridColumn {
        persistId: string;
    }

    export interface IKendoGridToolbarItem extends kendo.ui.GridToolbarItem {
        className?: string;
    }

    export interface IKendoGridOptions extends kendo.ui.GridOptions {
        toolbar?: IKendoGridToolbarItem[];
        columns?: IKendoGridColumn[];
    }

    export interface IKendoGrid extends kendo.ui.Grid {
        getOptions(): IKendoGridOptions;
        columns: IKendoGridColumn[];
    }

    export interface ILodashWithMixins extends _.LoDashStatic {
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
}
