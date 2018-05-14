module Kitos.GlobalAdmin.Organization {
    "use strict";

    export class OrganizationController {
        public mainGrid: IKendoGrid<Models.IOrganization>;
        public mainGridOptions: IKendoGridOptions<Models.IOrganization>;

        public static $inject: string[] = ['$rootScope', '$scope', '$http', 'notify', 'user', '_', '$', '$state', '$window', '$timeout'];

        constructor(private $rootScope, private $scope: ng.IScope, private $http, private notify, private user, private _, private $, private $state, private $window, private $timeout) {
            $rootScope.page.title = 'Organisationer';

            this.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: `/odata/Organizations?$expand=Type`,
                            dataType: "json"
                        },
                        destroy: {
                            url: (entity) => {
                                return `/odata/Organizations(${entity.Id})`;
                            },
                            dataType: "json",
                            contentType: "application/json"
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
                    schema: {
                        model: {
                            id: "Id"
                        }
                    }
                } as kendo.data.DataSourceOptions,
                    toolbar: [
                        {
                            name: "opretOrganisation",
                            text: "Opret Organisation",
                            template: "<a ui-sref='global-admin.organizations.create' class='btn btn-success pull-right'>#: text #</a>"
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
                editable: "popup",
                reorderable: true,
                resizable: true,
                filterable: {
                    mode: "row"
                },
                groupable: false,
                columnMenu: true,
                height: 750,
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
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        command: [
                            { text: "Redigér", click: this.onEdit, imageClass: "k-edit", className: "k-custom-edit", iconClass: "k-icon" } /* kendo typedef is missing imageClass and iconClass so casting to any */ as any,
                            { text: "Slet", click: this.onDelete, imageClass: "k-delete", className: "k-custom-delete", iconClass: "k-icon" } /* kendo typedef is missing imageClass and iconClass so casting to any */ as any,
                        ],
                        title: " ", width: 176,
                        persistId: "command"
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

        private onEdit = (e: JQueryEventObject) => {
            e.preventDefault();
            var dataItem = this.mainGrid.dataItem(this.$(e.currentTarget).closest("tr"));
            var entityId = dataItem["Id"];
            this.$state.go("global-admin.organizations.edit", { id: entityId });
        }

        private onDelete = (e: JQueryEventObject) => {
            e.preventDefault();
            var dataItem = this.mainGrid.dataItem(this.$(e.currentTarget).closest("tr"));

            if (this.$window.confirm("Er du sikker på at slette " + dataItem["Name"] + "?")) {
                this.mainGrid.dataSource.remove(dataItem);
                this.mainGrid.dataSource.sync();
            }
        }
    }

    angular
        .module("app")
        .config([
            '$stateProvider', ($stateProvider) => {
                $stateProvider.state('global-admin.organizations', {
                    url: '/organisations',
                    templateUrl: 'app/components/global-admin/global-admin-organizations.view.html',
                    controller: OrganizationController,
                    controllerAs: 'orgCtrl',
                    authRoles: ['GlobalAdmin'],
                    resolve: {
                        user: [
                            'userService', (userService) => {
                                return userService.getUser();
                            }
                        ]
                    }
                });
            }
        ]);
}