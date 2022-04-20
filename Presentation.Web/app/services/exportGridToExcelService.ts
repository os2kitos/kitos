module Kitos.Services.System {
    import WorkbookSheet = kendo.ooxml.WorkbookSheet;
    "use strict";

    export class ExportGridToExcelService {
        private exportFlag = false;
        static $inject = ["needsWidthFixService"];
        private columnIndexMap: {[key: number]: number};

        constructor(private readonly needsWidthFixService: NeedsWidthFix) { }

        getExcel(e: IKendoGridExcelExportEvent<any>, _: ILoDashWithMixins, timeout: ng.ITimeoutService, kendoGrid: IKendoGrid<any>) {
            const columns = e.sender.columns;
            const sheet = e.workbook.sheets[0];

            if (!this.exportFlag) {
                var columnsToShow = [];
                this.columnIndexMap = {};
                e.preventDefault();
                _.forEach(columns, (column, i) => {
                    if (column.attributes.parentId === undefined)
                        return;

                    //look for a parent column and check if any was found, and if any parent column should be visible
                    var columnsWithMatchingParentId = columns.filter(x => x.persistId === column.attributes.parentId);
                    if (columnsWithMatchingParentId.length < 1)
                        return;
                    if (this.checkIfAllColumnsAreHidden(columnsWithMatchingParentId))
                        return;

                    var index = columns.indexOf(columnsWithMatchingParentId[0]);
                    this.arrayMove(columns, column, i, index);

                    columnsToShow.push(column);
                });

                this.showSelectedColumns(columnsToShow, e);
                this.mapIndexes(columns);

                timeout(() => {
                    this.exportFlag = true;
                    e.sender.saveAsExcel();
                });
                
                return;
            }

            this.exportFlag = false;

            // hide columns on visual grid
            _.forEach(columns, column => {
                if (column.tempVisual) {
                    delete column.tempVisual;
                    e.sender.hideColumn(column);
                }
            });

            // render templates
            // skip header row
            for (let rowIndex = 1; rowIndex < sheet.rows.length; rowIndex++) {
                const row = sheet.rows[rowIndex];

                // -1 as sheet has header and dataSource doesn't
                const dataItem = e.data[rowIndex - 1];

                //todo cell indexes are not equal to columns array indexes so data will be incorrect.
                for (let columnIndex = 0; columnIndex < row.cells.length; columnIndex++) {
                    const mappedIndex = this.columnIndexMap[columnIndex];
                    if (columns[mappedIndex].field === "" || columns[mappedIndex].hidden) continue;
                    const cell = row.cells[mappedIndex];

                    const template = this.getTemplateMethod(columns[mappedIndex]);

                    let computedValue = template(dataItem);
                    if (computedValue == null) {
                        computedValue = "";
                    }
                    cell.value = computedValue;
                }
            }

            // hide loadingbar when export is finished
            kendo.ui.progress(kendoGrid.element, false);
            this.needsWidthFixService.fixWidth();
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
            _.forEach(columns,
                column => {
                    column.tempVisual = true;
                    e.sender.showColumn(column);
                }
            );
        }

        private arrayMove(columns: IKendoGridColumn<any>[], element: IKendoGridColumn<any>, fromIndex: number, toIndex: number) {
            //we want to move the related column to "right" side of the base column
            toIndex += 1;

            columns.splice(fromIndex, 1);
            columns.splice(toIndex, 0, element);
            
            this.columnIndexMap[fromIndex] = toIndex + 1;
            for (let i = toIndex + 1; i < fromIndex; i++) {
                this.columnIndexMap[i] = i + 1;
            }
        }

        private mapIndexes(columns: IKendoGridColumn<any>[]) {
            columns.forEach(column => {
                var currentIndex = columns.indexOf(column);
                var test = typeof this.columnIndexMap[currentIndex];
                var test2 = this.columnIndexMap[currentIndex];
                if (this.columnIndexMap[currentIndex] !== undefined)
                    return;

                this.columnIndexMap[currentIndex] = currentIndex;
            }, this);
        }
    }

    app.service("exportGridToExcelService", ExportGridToExcelService);
}