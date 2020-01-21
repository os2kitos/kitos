module Kitos.LocalAdmin.Organization {
    "use strict";

    export class OrganizationController {
        public mainGrid: IKendoGrid<Models.IOrganization>;
        public mainGridOptions: IKendoGridOptions<Models.IOrganization>;

        public static $inject: string[] = ["$scope", "$http", "notify", "$timeout", "_","exportGridToExcelService"];

        constructor(private $scope, private $http, private notify, private $timeout, private _, private exportGridToExcelService) {
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

        private exportToExcel = (e: IKendoGridExcelExportEvent<Models.IOrganizationRight>) => {
            this.exportGridToExcelService.getExcel(e, this._, this.$timeout, this.mainGrid);
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
