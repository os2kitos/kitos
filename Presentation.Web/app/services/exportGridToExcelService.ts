module Kitos.Services.System {
    "use strict";

    export interface IKendoGridAdditionalExcelColumn {
        title: string;
        persistId: string;
        width: number;
        template: (dataItem: any) => string;
        placeAfterPersistId? : string;
    }

    export class ExportGridToExcelService {
        private exportFlag = false;
        static $inject = ["needsWidthFixService"];

        constructor(private needsWidthFixService: NeedsWidthFix) { }

        public getExcel(e: IKendoGridExcelExportEvent<any>, _: ILoDashWithMixins, timeout: ng.ITimeoutService, kendoGrid: IKendoGrid<any>, additionalColumns? : IKendoGridAdditionalExcelColumn[]) {
            var columns = e.sender.columns;

            if (!this.exportFlag) {
                e.preventDefault();
                _.forEach(columns, column => {
                    if (column.hidden) {
                        column.tempVisual = true;
                        e.sender.showColumn(column);
                    }
                });
                timeout(() => {
                    this.exportFlag = true;
                    e.sender.saveAsExcel();
                });
            } else {
                this.exportFlag = false;

                // hide coloumns on visual grid
                _.forEach(columns, column => {
                    if (column.tempVisual) {
                        delete column.tempVisual;
                        e.sender.hideColumn(column);
                    }
                });

                // render templates
                var sheet = e.workbook.sheets[0];
                if (additionalColumns && additionalColumns.length > 0) {
                    //take copy and modify that one - otherwise the source grid is modified
                    columns = [...columns];

                    const addedColumns = [];
                    //Extend the grid with excel-only columns
                    for (let column of additionalColumns) {
                        //TODO: Add compute "insert at id"
                        const additionalColumn = {
                            title: column.title,
                            persistId: column.persistId,
                            field: column.persistId,
                            excelTemplate: column.template,
                            width: column.width
                        };

                        //Add to grid columns
                        columns.push(additionalColumn);

                        //Add sheet column //TODO: At correct index
                        sheet.columns.push({ width: column.width, autoWidth: false });

                        //Track added columns for extending the sheet
                        addedColumns.push(additionalColumns);
                    }

                    for (let ri = 0; ri < sheet.rows.length; ri++) {
                        for (let column of additionalColumns) {
                            const isHeaderRow = ri === 0;
                            if (isHeaderRow) {
                                sheet.rows[ri].cells.push({
                                    background: '#7a7a7a',
                                    color: '#fff',
                                    value: column.title,
                                    colSpan: 1,
                                    rowSpan: 1
                                });
                            } else {
                                sheet.rows[ri].cells.push({
                                    value: "",
                                });
                            }
                        }
                    }
                }
                
                // skip header row
                for (let rowIndex = 1; rowIndex < sheet.rows.length; rowIndex++) {
                    const row = sheet.rows[rowIndex];

                    // -1 as sheet has header and dataSource doesn't
                    const dataItem = e.data[rowIndex - 1];

                    for (let columnIndex = 0; columnIndex < row.cells.length; columnIndex++) {
                        if (columns[columnIndex].field === "") continue;
                        const cell = row.cells[columnIndex];

                        const template = this.getTemplateMethod(columns[columnIndex]);

                        cell.value = template(dataItem);
                    }
                }

                //TODO: Cleanup after rendering?

                // hide loadingbar when export is finished
                kendo.ui.progress(kendoGrid.element, false);
                this.needsWidthFixService.fixWidth();
            }
        }

        private getTemplateMethod(column) {
            var template: Function;

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
    }

    app.service("exportGridToExcelService", ExportGridToExcelService);
}