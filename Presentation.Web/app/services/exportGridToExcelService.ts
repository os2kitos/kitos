module Kitos.Services.System {
    import ArrayHelper = Helpers.ArrayHelper;
    "use strict";

    export class ExportGridToExcelService {
        private exportFlag = false;
        static $inject = ["needsWidthFixService"];
        private columnsToShow: Array<{ columnId: string, index?: number, parentId?: string, indexBeforeMoving?: number, indexAfterMoving?: number }> = [];
        private originalColumns: IKendoGridColumn<any>[];

        constructor(private readonly needsWidthFixService: NeedsWidthFix) { }

        getExcel(e: IKendoGridExcelExportEvent<any>, _: ILoDashWithMixins, timeout: ng.ITimeoutService, kendoGrid: IKendoGrid<any>) {
            const sheet = e.workbook.sheets[0];
            var columns = e.sender.columns;

            if (!this.exportFlag) {
                this.originalColumns = JSON.parse(JSON.stringify(columns));

                e.preventDefault();
                _.forEach(columns, (column, i) => {
                    if (!column.hidden) {
                        this.columnsToShow.push({ columnId: column.persistId });
                        return;
                    }
                    if (column.attributes.parentId === undefined)
                        return;

                    //look for a parent column and check if any was found, and if any parent column should be visible
                    var columnsWithMatchingParentId = columns.filter(x => x.persistId === column.attributes.parentId);
                    if (columnsWithMatchingParentId.length < 1)
                        return;
                    if (this.checkIfAllColumnsAreHidden(columnsWithMatchingParentId))
                        return;

                    //this.showSelectedColumn(column, e);
                    var index = columns.indexOf(columnsWithMatchingParentId[0]);

                    this.columnsToShow.push({ columnId: column.persistId, parentId: column.attributes.parentId, indexBeforeMoving: i, indexAfterMoving: index + 1});
                    ArrayHelper.arrayMoveElementToRightSide(columns, i, index);
                });

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


            // render templates
            // skip header row
            for (let rowIndex = 1; rowIndex < sheet.rows.length; rowIndex++) {
                const row = sheet.rows[rowIndex];

                // -1 as sheet has header and dataSource doesn't
                const dataItem = e.data[rowIndex - 1];

                //todo cell indexes are not equal to columns array indexes so data will be incorrect.
                for (let columnIndex = 0; columnIndex < row.cells.length; columnIndex++) {
                    const mappedIndex = this.columnsToShow.indexOf(this.columnsToShow[columnIndex]);
                    const columnOriginalIndex = this.columnsToShow[columnIndex].index;
                    if (columns[columnOriginalIndex].field === "" || columns[columnOriginalIndex].hidden) continue;
                    const cell = row.cells[mappedIndex];

                    const template = this.getTemplateMethod(columns[columnOriginalIndex]);

                    let computedValue = template(dataItem);
                    if (computedValue == null) {
                        computedValue = "";
                    }
                    cell.value = computedValue;
                }
            }
            columns.filter(x => x.tempVisual).forEach(column => {
                var original = this.originalColumns.filter(x => x.persistId === column.persistId)[0];
                original.tempVisual = true;
            });
            columns = this.originalColumns;

            // hide columns on visual grid
            this.originalColumns.forEach(column => {
                if (column.tempVisual) {
                    delete column.tempVisual;
                    e.sender.hideColumn(column);
                    //10,12
                }
            });
            /*this.columnsToShow.forEach(column => {
                if (column.parentId === undefined)
                    return;

                var originalColumn = originalColumns.filter(x => x.persistId === column.columnId)[0];
                var movedColumn = columns.filter(x => x.persistId === column.columnId)[0];
                var originalIndex = originalColumns.indexOf(originalColumn);
                var movedIndex = columns.indexOf(movedColumn);
                ArrayHelper.arrayMoveElementTo(columns, movedIndex, originalIndex);
            });*/
            //columns = originalColumns;

            // hide loadingbar when export is finished
            kendo.ui.progress(kendoGrid.element, false);
            this.needsWidthFixService.fixWidth();

            this.columnsToShow = [];
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

        private checkIfAllColumnsAreHidden(columns: IKendoGridColumn<any>[]): boolean {
            let isHidden = true;
            columns.forEach(column => {
                if (!column.hidden) {
                    isHidden = false;
                    return;
                }
            });

            return isHidden;
        }

        private showSelectedColumns(columns: IKendoGridColumn<any>[], e: IKendoGridExcelExportEvent<any>) {
            _.forEach(this.columnsToShow,
                column => {
                    if (column.parentId === undefined)
                        return;

                    const columnToShow = columns.filter(x => x.persistId === column.columnId)[0];
                    if (columnToShow === undefined || columnToShow === null)
                        return;

                    columnToShow.tempVisual = true;
                    e.sender.showColumn(columnToShow);
                }
            );
        }

        private showSelectedColumn(column: IKendoGridColumn<any>, e: IKendoGridExcelExportEvent<any>) {
            column.tempVisual = true;
            e.sender.showColumn(column);
        }

        private sortColumnArray() {
            this.columnsToShow.forEach((column, i)=> {
                if (column.parentId === undefined)
                    return;

                var parentColumn = this.columnsToShow.filter(x => x.columnId === column.parentId)[0];
                var parentIndex = this.columnsToShow.indexOf(parentColumn);
                ArrayHelper.arrayMoveElementToRightSide(this.columnsToShow, i, parentIndex);
            });
        }

        private mapIndexes(columns: IKendoGridColumn<any>[]) {
            columns.forEach((column, i)=> {
                var selectedColumn = this.columnsToShow.filter(x => x.columnId === column.persistId)[0];
                if (selectedColumn === undefined || selectedColumn === null)
                    return;

                selectedColumn.index = i;
            }, this);
        }
    }

    app.service("exportGridToExcelService", ExportGridToExcelService);
}