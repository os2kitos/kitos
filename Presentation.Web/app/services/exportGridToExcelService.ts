﻿module Kitos.Services.System {
    import ArrayHelper = Helpers.ArrayHelper;
    import IExcelConfig = Models.IExcelConfig;
    "use strict";

    export class ExportGridToExcelService {
        private exportFlag = false;
        static $inject = ["needsWidthFixService"];
        private columnsToShow: Array<{ columnId: string, index?: number, parentId?: string }> = [];

        constructor(private readonly needsWidthFixService: NeedsWidthFix) { }

        getExcel(e: IKendoGridExcelExportEvent<any>, _: ILoDashWithMixins, timeout: ng.ITimeoutService, kendoGrid: IKendoGrid<any>, config?: IExcelConfig) {
            const columns = e.sender.columns;

            if (!this.exportFlag) {
                e.preventDefault();
                const onlyVisibleColumns = config?.onlyVisibleColumns === true;

                this.selectColumnsToDisplay(e, columns, onlyVisibleColumns);

                this.showSelectedColumns(columns, e);
                this.sortColumnArray();
                this.mapIndexes(columns);

                timeout(() => {
                    this.exportFlag = true;
                    e.sender.saveAsExcel();
                });

                return;
            }

            this.exportFlag = false;
            const sheet = e.workbook.sheets[0];

            // render templates
            // skip header row
            for (let rowIndex = 1; rowIndex < sheet.rows.length; rowIndex++) {
                const row = sheet.rows[rowIndex];

                // -1 as sheet has header and dataSource doesn't
                const dataItem = e.data[rowIndex - 1];

                for (let columnIndex = 0; columnIndex < row.cells.length; columnIndex++) {
                    const columnOriginalIndex = this.columnsToShow[columnIndex].index;
                    if (columnOriginalIndex === undefined) continue;
                    if (columns[columnOriginalIndex].field === "" || columns[columnOriginalIndex].hidden) continue;
                    const cell = row.cells[columnIndex];

                    const template = this.getTemplateMethod(columns[columnOriginalIndex]);

                    let computedValue = template(dataItem);
                    if (computedValue == null) {
                        computedValue = "";
                    }
                    cell.value = computedValue;
                }
            }

            // hide columns on visual grid
            columns.forEach(column => {
                if (column.tempVisual) {
                    delete column.tempVisual;
                    e.sender.hideColumn(column);
                }
            });

            // hide loadingbar when export is finished
            kendo.ui.progress(kendoGrid.element, false);
            this.needsWidthFixService.fixWidth();

            this.columnsToShow = [];
        }

        private selectColumnsToDisplay(e: IKendoGridExcelExportEvent<any>, columns: IKendoGridColumn<any>[], exportOnlyVisibleColumns: boolean) {
            if (!exportOnlyVisibleColumns) {
                this.showAllRootColumns(e, columns);
            }

            this.mapShownColumns(columns);
            this.moveRelatedColumns(columns, e);
        }

        private showAllRootColumns(e: IKendoGridExcelExportEvent<any>, columns: IKendoGridColumn<any>[]) {
            _.forEach(columns,
                column => {
                    if (column.hidden && column.parentId === undefined) {
                        column.tempVisual = true;
                        e.sender.showColumn(column);
                    }
                });
        }

        private getTemplateMethod(column) {
            let template: Function;

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

        private showSelectedColumns(columns: IKendoGridColumn<any>[], e: IKendoGridExcelExportEvent<any>) {
            _.forEach(this.columnsToShow,
                column => {
                    if (column.parentId === undefined)
                        return;
                    var parentColumn = columns.filter(x => x.persistId === column.parentId)[0];
                    if (parentColumn === undefined) {
                        const errorMsg = `Error ParentColumn with id ${column.parentId} was not found`;
                        console.error(errorMsg);
                        throw new Error(errorMsg);
                    }

                    const columnToShow = columns.filter(x => x.persistId === column.columnId)[0];
                    if (columnToShow === undefined || columnToShow === null)
                        return;

                    columnToShow.tempVisual = true;
                    e.sender.showColumn(columnToShow);
                }
            );
        }

        private mapShownColumns(columns: IKendoGridColumn<any>[]) {
            columns.filter(x => !x.hidden).forEach(column => {
                this.columnsToShow.push({ columnId: column.persistId });
            }, this);
        }

        private moveRelatedColumns(columns: IKendoGridColumn<any>[], e: IKendoGridExcelExportEvent<any>) {
            columns.filter(x => x.hidden).forEach(column => {
                if (column.parentId === undefined)
                    return;
                //look for a parent column and check if any was found, and if any parent column should be visible
                var columnsWithMatchingParentId = columns.filter(x => x.persistId === column.parentId);
                if (columnsWithMatchingParentId.length !== 1) {
                    if (columnsWithMatchingParentId.length === 0) {

                    } else {
                        console.error(
                            "Column ",
                            column.persistId,
                            " has multiple(",
                            columnsWithMatchingParentId.length,
                            ") columns with matching parentId (",
                            column.parentId, "):",
                            columnsWithMatchingParentId.map(x => x.persistId).join(","));
                    }

                    return;
                }
                const parentColumn = columnsWithMatchingParentId[0];
                if (parentColumn.hidden)
                    return;

                var parentIndex = columns.indexOf(parentColumn);

                this.columnsToShow.push({ columnId: column.persistId, parentId: column.parentId });

                //if the related column is on the "left" side of the parent don't increment parentIndex by 1, as the indexes will be decreased by 1 when the related column is moved
                const addNumberToParentIndex = (columns.indexOf(column) > parentIndex) ? 1 : 0;
                e.sender.reorderColumn(parentIndex + addNumberToParentIndex, column);
            }, this);
        }

        private sortColumnArray() {
            this.columnsToShow.forEach((column, i) => {
                if (column.parentId === undefined)
                    return;

                var parentColumn = this.columnsToShow.filter(x => x.columnId === column.parentId)[0];
                var parentIndex = this.columnsToShow.indexOf(parentColumn);
                ArrayHelper.arrayMoveElementToRightSide(this.columnsToShow, i, parentIndex);
            }, this);
        }

        private mapIndexes(columns: IKendoGridColumn<any>[]) {
            columns.forEach((column, i) => {
                var selectedColumn = this.columnsToShow.filter(x => x.columnId === column.persistId)[0];
                if (selectedColumn === undefined || selectedColumn === null)
                    return;

                selectedColumn.index = i;
            }, this);
        }
    }

    app.service("exportGridToExcelService", ExportGridToExcelService);
}