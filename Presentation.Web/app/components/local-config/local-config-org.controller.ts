module Kitos.LocalAdmin.Organization {
    "use strict";

    export class OrganizationController {
        public mainGrid: IKendoGrid<Models.IOrganization>;
        public mainGridOptions: IKendoGridOptions<Models.IOrganization>;

        public static $inject: string[] = ["$scope", "$http", "notify", "$timeout", "_"];

        constructor(private $scope, private $http, private notify, private $timeout, private _) {
            this.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: `/odata/Organizations?$expand=Type`,
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
                } as kendo.data.DataSourceOptions,
                toolbar: [
                    {
                        name: "opretOrganisation",
                        text: "Opret Organisation",
                        template: "<a ui-sref='local-config.org.create' class='btn btn-success pull-right'>#: text #</a>"
                    },
                    { name: "excel", text: "Eksportér til Excel", className: "pull-right" }
                ],
                excel: {
                    fileName: "Organisationer.xlsx",
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
                editable: true,
                reorderable: true,
                resizable: true,
                filterable: {
                    mode: "row"
                },
                groupable: false,
                columnMenu: true,
                height: window.innerHeight - 200,
                excelExport: this.exportToExcel,
                columns: [
                    {
                        field: "Name", title: "Navn", width: 230,
                        persistId: "name", // DON'T YOU DARE RENAME!,
                        hidden: false,
                        excelTemplate: (dataItem) => dataItem.Name,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Cvr", title: "CVR", width: 230,
                        persistId: "cvr", // DON'T YOU DARE RENAME!
                        hidden: false,
                        excelTemplate: (dataItem) => dataItem.Cvr,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Type.Name", title: "Type", width: 230,
                        persistId: "type", // DON'T YOU DARE RENAME!
                        hidden: false,
                        template: (dataItem) => dataItem.Type.Name,
                        excelTemplate: (dataItem) => dataItem.Type.Name,
                        filterable: {
                            cell: {
                                template: function (args) {
                                    args.element.kendoDropDownList({
                                        dataSource: [{ type: "Kommune", value: "Kommune" }, { type: "Interessefællesskab", value: "Interessefællesskab" }, { type: "Virksomhed", value: "Virksomhed" }, { type: "Anden offentlig myndighed", value: "Anden offentlig myndighed" }],
                                        dataTextField: "type",
                                        dataValueField: "value",
                                        valuePrimitive: true
                                    });
                                },
                                showOperators: false
                            }
                        }
                    },
                    {
                        field: "AccessModifier", title: "Synlighed", width: 230,
                        persistId: "synlighed", // DON'T YOU DARE RENAME!
                        hidden: false,
                        template: `<display-access-modifier value="dataItem.AccessModifier"></display-access-modifier>`,
                        excelTemplate: (dataItem) => dataItem.AccessModifier.toString(),
                        filterable: {
                            cell: {
                                template: function (args) {
                                    args.element.kendoDropDownList({
                                        dataSource: [{ type: "Lokal", value: "Local" }, { type: "Offentlig", value: "Public" }],
                                        dataTextField: "type",
                                        dataValueField: "value",
                                        valuePrimitive: true
                                    });
                                },
                                showOperators: false
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
        }

        private exportFlag = false;
        private exportToExcel = (e: IKendoGridExcelExportEvent<Models.IOrganizationRight>) => {
            var columns = e.sender.columns;

            if (!this.exportFlag) {
                e.preventDefault();
                this._.forEach(columns, column => {
                    if (column.hidden) {
                        column.tempVisual = true;
                        e.sender.showColumn(column);
                    }
                });
                this.$timeout(() => {
                    this.exportFlag = true;
                    e.sender.saveAsExcel();
                });
            } else {
                this.exportFlag = false;

                // hide columns on visual grid
                this._.forEach(columns, column => {
                    if (column.tempVisual) {
                        delete column.tempVisual;
                        e.sender.hideColumn(column);
                    }
                });

                // render templates
                const sheet = e.workbook.sheets[0];

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

                // hide loading bar when export is finished
                kendo.ui.progress(this.mainGrid.element, false);
            }
        }

        private getTemplateMethod(column) {
            let template: Function;

            if (column.excelTemplate) {
                template = column.excelTemplate;
            } else if (typeof column.template === "function") {
                template = (column.template as Function);
            } else if (typeof column.template === "string") {
                template = kendo.template(column.template as string);
            } else {
                template = t => t;
            }

            return template;
        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", ($stateProvider) => {
                $stateProvider.state("local-config.org", {
                    url: "/org",
                    templateUrl: "app/components/local-config/local-config-org.view.html",
                    controller: OrganizationController,
                    controllerAs: "orgCtrl",
                    authRoles: [Models.OrganizationRole.LocalAdmin],
                    resolve: {
                        user: [
                            "userService", (userService) => {
                                return userService.getUser();
                            }
                        ]
                    }
                });
            }]);
}
