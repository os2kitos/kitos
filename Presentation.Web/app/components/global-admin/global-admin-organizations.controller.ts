module Kitos.GlobalAdmin.Organization {
    "use strict";

    export class OrganizationController {
        public mainGrid: IKendoGrid<Models.IOrganization>;
        public mainGridOptions: IKendoGridOptions<Models.IOrganization>;

        public static $inject: string[] = ['$rootScope', '$scope', '$http', 'notify', 'user', '_', '$', '$state', '$window', '$timeout','exportGridToExcelService'];

        constructor(private $rootScope, private $scope: ng.IScope, private $http, private notify, private user, private _, private $, private $state, private $window, private $timeout, private exportGridToExcelService) {
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
                            template: "<a ui-sref='global-admin.organizations.create' data-element-type='createNewOrgButton' class='btn btn-success pull-right'>#: text #</a>"
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

        private reload() {
            this.$state.go(".", null, { reload: true });
        }

        private exportToExcel = (e: IKendoGridExcelExportEvent<Models.IOrganizationRight>) => {
            this.exportGridToExcelService.getExcel(e, this._, this.$timeout, this.mainGrid);
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
                this.$http.delete(`odata/Organizations(${dataItem["Id"]})`);
                this.reload();
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