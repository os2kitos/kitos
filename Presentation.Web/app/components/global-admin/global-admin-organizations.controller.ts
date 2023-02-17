module Kitos.GlobalAdmin.Organization {
    import IGridViewAccess = Utility.KendoGrid.IGridViewAccess;
    import IExcelConfig = Models.IExcelConfig;
    "use strict";

    export class OrganizationController {
        mainGrid: IKendoGrid<Models.IOrganization>;
        mainGridOptions: IKendoGridOptions<Models.IOrganization>;

        static $inject: string[] = ["$rootScope", "_", "$", "$state", "$timeout", "exportGridToExcelService"];

        constructor(
            private readonly $rootScope,
            private readonly _,
            private readonly $,
            private readonly $state,
            private readonly $timeout,
            private readonly exportGridToExcelService) {
            $rootScope.page.title = "Organisationer";

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
                        template:
                            "<a ui-sref='global-admin.organizations.create' data-element-type='createNewOrgButton' class='btn kendo-btn-sm btn-success pull-right kendo-margin-left'>#: text #</a>"
                    }
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
                height: window.innerHeight - 150,
                excelExport: (e: any) => this.exportToExcel(e),
                columns: [
                    {
                        field: "Name",
                        title: "Navn",
                        width: 300,
                        persistId: "name",
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
                        field: "Cvr",
                        title: "CVR",
                        width: 160,
                        persistId: "cvr", 
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
                        field: "Type.Name",
                        title: "Type",
                        width: 160,
                        persistId: "type", 
                        hidden: false,
                        template: (dataItem) => dataItem.Type.Name,
                        excelTemplate: (dataItem) => dataItem.Type.Name,
                        filterable: {
                            cell: {
                                template: function (args) {
                                    args.element.kendoDropDownList({
                                        dataSource: [
                                            { type: "Kommune", value: "Kommune" },
                                            { type: "Interessefællesskab", value: "Interessefællesskab" },
                                            { type: "Virksomhed", value: "Virksomhed" },
                                            { type: "Anden offentlig myndighed", value: "Anden offentlig myndighed" }
                                        ],
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
                        field: "ForeignCvr",
                        title: "Udenlandsk virksomhed",
                        width: 200,
                        persistId: "foreignCvr",
                        hidden: false,
                        excelTemplate: (dataItem) => dataItem.ForeignCvr,
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
                            {
                                text: "Redigér",
                                click: this.onEdit,
                                imageClass: "k-edit",
                                className: "k-custom-edit",
                                iconClass: "k-icon"
                            } /* kendo typedef is missing imageClass and iconClass so casting to any */ as any,
                            {
                                text: "Slet",
                                click: this.onDelete,
                                imageClass: "k-delete",
                                className: "k-custom-delete",
                                iconClass: "k-icon"
                            } /* kendo typedef is missing imageClass and iconClass so casting to any */ as any,
                        ],
                        title: " ",
                        width: 176,
                        persistId: "command"
                    }
                ]
            };

            Helpers.ExcelExportHelper.setupExcelExportDropdown(() => this.excelConfig,
                () => this.mainGrid,
                this.$rootScope,
                this.mainGridOptions.toolbar);

            function customFilter(args) {
                args.element.kendoAutoComplete({
                    noDataTemplate: ""
                });
            }
        }

        private reload() {
            this.$state.go(".", null, { reload: true });
        }

        private readonly excelConfig: IExcelConfig = {
        };

        private exportToExcel = (e: IKendoGridExcelExportEvent<Models.IOrganizationRight>) => {
            this.exportGridToExcelService.getExcel(e, this._, this.$timeout, this.mainGrid, this.excelConfig);
        }

        private getSenderGridItemId(e: JQueryEventObject) {
            const dataItem = this.mainGrid.dataItem(this.$(e.currentTarget).closest("tr"));
            return  dataItem["Id"];
        }

        private onEdit = (e: JQueryEventObject) => {
            e.preventDefault();
            var entityId = this.getSenderGridItemId(e);
            this.$state.go("global-admin.organizations.edit", { id: entityId });
        }

        private onDelete = (e: JQueryEventObject) => {
            e.preventDefault();
            var entityId = this.getSenderGridItemId(e);
            this.$state.go("global-admin.organizations.delete", { id: entityId });
        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", ($stateProvider) => {
                $stateProvider.state("global-admin.organizations", {
                    url: "/organisations",
                    templateUrl: "app/components/global-admin/global-admin-organizations.view.html",
                    controller: OrganizationController,
                    controllerAs: "orgCtrl",
                    authRoles: ["GlobalAdmin"]
                });
            }
        ]);
}