module Kitos.Services.System {
    "use strict";

    export interface IKendoGridAdditionalExcelColumn {
        title: string;
        persistId: string;
        width: number;
        template: (dataItem: any) => string;
        dependOnColumnPersistId: string | null;
    }

    export class ExportGridToExcelService {
        private exportFlag = false;
        static $inject = ["needsWidthFixService"];

        constructor(private needsWidthFixService: NeedsWidthFix) { }

        public getExcel(e: IKendoGridExcelExportEvent<any>, _: ILoDashWithMixins, timeout: ng.ITimeoutService, kendoGrid: IKendoGrid<any>, additionalColumns?: IKendoGridAdditionalExcelColumn[]) {
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

                    const columnInsertedByDependency = {};
                    const columnDependencyMap = additionalColumns.reduce((acc, next) => {
                        if (next.dependOnColumnPersistId !== null) {
                            if (!acc[next.dependOnColumnPersistId]) {
                                acc[next.dependOnColumnPersistId] = next;
                            } else {
                                throw `Error existing dependency on column ${next.dependOnColumnPersistId} already found.${acc[next.dependOnColumnPersistId]} depends on it`;
                            }
                        }
                        return acc;
                    }, {});

                    //Make sure insertion order is correct for the columns that depend on one another
                    const additionalColumnsOrgOrder = [...additionalColumns];
                    additionalColumns = [];
                    for (let column of additionalColumnsOrgOrder) {
                        if (columnInsertedByDependency[column.persistId] !== true) {
                            additionalColumns.push(column);

                            let depender = columnDependencyMap[column.persistId];
                            if (!!depender) {
                                additionalColumns.push(depender);
                                columnInsertedByDependency[depender.persistId] = true;
                            }
                        }
                    }

                    const additionalColumnToIndexMap = {};
                    for (let i = 0; i < additionalColumns.length; i++) {
                        additionalColumnToIndexMap[additionalColumns[i].persistId] = i;
                    }

                    //Extend the grid with excel-only columns
                    var initialIndexToTargetIndex = {}; //Maps original sheet index to final index (ordered by dependency graph)

                    //Start setting the additional columns to the tail of the list
                    for (let i = 0; i < additionalColumns.length; i++) {
                        initialIndexToTargetIndex[i + columns.length] = i + columns.length;
                    }

                    var indexOffset = 0;
                    for (let i = 0; i < columns.length; i++) {
                        let column = columns[i];
                        initialIndexToTargetIndex[i] = i + indexOffset;
                        let depender = columnDependencyMap[column.persistId];
                        if (!!depender) {
                            let dependerIndex = columns.length + additionalColumnToIndexMap[depender.persistId];
                            indexOffset++;
                            initialIndexToTargetIndex[dependerIndex] = i + indexOffset;
                        }
                    }

                    //Add all additional columns to the tail
                    for (let i = 0; i < additionalColumns.length; i++) {
                        let column = additionalColumns[i];
                        const additionalColumn = {
                            title: column.title,
                            persistId: column.persistId,
                            field: column.persistId,
                            excelTemplate: column.template,
                            width: column.width
                        };

                        //Add to grid columns
                        columns.push(additionalColumn);

                        //Add sheet column
                        sheet.columns.push({ width: column.width, autoWidth: false });
                    }

                    //Reorder columns and sheet columns
                    var orgColumns = [...columns];
                    var orgSheetColumns = [...sheet.columns];
                    for (let i = 0; i < columns.length; i++) {
                        //Swap the indices
                        columns[initialIndexToTargetIndex[i]] = orgColumns[i];
                        sheet.columns[initialIndexToTargetIndex[i]] = orgSheetColumns[i];
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

                        //Reorder according to dependency grapth
                        let orgCells = [...sheet.rows[ri].cells];
                        for (let i = 0; i < sheet.rows[ri].cells.length; i++) {
                            sheet.rows[ri].cells[initialIndexToTargetIndex[i]] = orgCells[i];
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