module Kitos.Organization.Users {
    "use strict";

    interface IGridModel extends Models.IUser {
        canEdit: boolean;
        isLocalAdmin: boolean;
        isOrgAdmin: boolean;
        isProjectAdmin: boolean;
        isSystemAdmin: boolean;
        isContractAdmin: boolean;
        isReportAdmin: boolean;
    }

    class OrganizationUserController {
        public mainGrid: IKendoGrid<IGridModel>;
        public mainGridOptions: IKendoGridOptions<IGridModel>;

        public static $inject: string[] = ["$http", "$timeout", "_", "$", "$state", "$scope", "notify", "user", "hasWriteAccess"];

        constructor(
            private $http: ng.IHttpService,
            private $timeout: ng.ITimeoutService,
            private _: ILoDashWithMixins,
            private $: JQueryStatic,
            private $state: ng.ui.IStateService,
            private $scope,
            private notify,
            private user,
            private hasWriteAccess) {
            this.hasWriteAccess = hasWriteAccess;
            this.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: `/odata/Users?$expand=ObjectOwner,OrganizationRights($filter=OrganizationId eq ${this.user.currentOrganizationId})`,
                            dataType: "json"
                        },
                        destroy: {
                            url: (entity) => {
                                return `/odata/Organizations(${this.user.currentOrganizationId})/RemoveUser()`;
                            },
                            dataType: "json",
                            contentType: "application/json"
                        },
                        parameterMap: (options, operation) => {
                            if (operation === "read") {
                                // get kendo to map parameters to an odata url
                                const parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, operation);
                                //parameterMap.$filter = "";
                                if (parameterMap.$filter) {
                                    parameterMap.$filter = this.fixNameFilter(parameterMap.$filter, "Name");
                                    parameterMap.$filter = this.fixNameFilter(parameterMap.$filter, "ObjectOwner.Name");
                                }
                                //parameterMap =+ `AND OrganizationRights/any(x: x/OrganizationId eq ${this.user.currentOrganizationId})`;
                                return parameterMap;
                            }

                            if (operation === "destroy") {
                                var requestBody = {
                                    userId: options["Id"]
                                };
                                return kendo.stringify(requestBody);
                            }

                            return kendo.stringify(options);
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
                            id: "Id",
                            fields: {
                                Name: { type: "text" },
                                LastName: { type: "text" },
                                Email: { type: "text" },
                                LastAdvisDate: { type: "date" }
                            }
                        },
                        parse: response => {
                            // iterate each user
                            this._.forEach(response.value, (usr: IGridModel) => {
                                // set if the user can edit
                                if (this.user.isGlobalAdmin || this.user.isLocalAdmin) {
                                    usr.canEdit = true;
                                } else if (this.user.id === usr.Id) {
                                    usr.canEdit = true;
                                } else {
                                    usr.canEdit = false;
                                }

                                // remove the user role
                                this._.remove(usr.OrganizationRights, (right) => right.Role === Models.OrganizationRole.User);

                                usr.isLocalAdmin = this._.find(usr.OrganizationRights, (right) => right.Role === Models.OrganizationRole.LocalAdmin) !== undefined;
                                usr.isOrgAdmin = this._.find(usr.OrganizationRights, (right) => right.Role === Models.OrganizationRole.OrganizationModuleAdmin) !== undefined;
                                usr.isProjectAdmin = this._.find(usr.OrganizationRights, (right) => right.Role === Models.OrganizationRole.ProjectModuleAdmin) !== undefined;
                                usr.isSystemAdmin = this._.find(usr.OrganizationRights, (right) => right.Role === Models.OrganizationRole.SystemModuleAdmin) !== undefined;
                                usr.isContractAdmin = this._.find(usr.OrganizationRights, (right) => right.Role === Models.OrganizationRole.ContractModuleAdmin) !== undefined;
                                usr.isReportAdmin = this._.find(usr.OrganizationRights, (right) => right.Role === Models.OrganizationRole.ReportModuleAdmin) !== undefined;
                            });
                            return response;
                        }
                    }
                } as kendo.data.DataSourceOptions,
                excel: {
                    fileName: "Brugere.xlsx",
                    filterable: true,
                    allPages: true
                },
                pageable: {
                    refresh: true,
                    pageSizes: [10, 25, 50, 100, 200],
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
                columnMenu: {
                    filterable: false
                },
                detailTemplate: (dataItem) => `<uib-tabset active="0">
                    <uib-tab index="0" heading="Organisation roller"><user-organization-unit-roles user-id="${dataItem.Id}" current-organization-id="${this.user.currentOrganizationId}"></user-organization-unit-roles></uib-tab>
                    <uib-tab index="1" heading="Projekt roller"><user-project-roles user-id="${dataItem.Id}" current-organization-id="${this.user.currentOrganizationId}"></user-project-roles></uib-tab>
                    <uib-tab index="2" heading="System roller"><user-system-roles user-id="${dataItem.Id}" current-organization-id="${this.user.currentOrganizationId}"></user-system-roles></uib-tab>
                    <uib-tab index="3" heading="Kontrakt roller"><user-contract-roles user-id="${dataItem.Id}" current-organization-id="${this.user.currentOrganizationId}"></user-contract-roles></uib-tab>
                </uib-tabset>`,
                excelExport: this.exportToExcel,
                columns: [
                    {
                        field: "Name", title: "Navn", width: 230,
                        persistId: "fullname", // DON'T YOU DARE RENAME!
                        template: (dataItem) => `${dataItem.Name} ${dataItem.LastName}`,
                        excelTemplate: (dataItem) => `${dataItem.Name} ${dataItem.LastName}`,
                        hidden: false,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Email", title: "Email", width: 230,
                        persistId: "email", // DON'T YOU DARE RENAME!
                        template: (dataItem) => dataItem.Email,
                        excelTemplate: (dataItem) => dataItem.Email,
                        hidden: false,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "LastAdvisDate", title: "Advis", width: 110,
                        persistId: "advisdate", // DON'T YOU DARE RENAME!
                        template: (dataItem) => `<advis-button data-user="dataItem" data-current-organization-id="${this.user.currentOrganizationId}" data-ng-disabled="${!dataItem.canEdit}"></advis>`,
                        excelTemplate: (dataItem) => dataItem.LastAdvisDate ? dataItem.LastAdvisDate.toDateString() : "",
                        hidden: false,
                        filterable: false
                    },
                    {
                        field: "ObjectOwner.Name", title: "Oprettet af", width: 150,
                        persistId: "createdby", // DON'T YOU DARE RENAME!
                        template: (dataItem) => dataItem.ObjectOwner ? `${dataItem.ObjectOwner.Name} ${dataItem.ObjectOwner.LastName}` : "",
                        excelTemplate: (dataItem) => dataItem.ObjectOwner ? `${dataItem.ObjectOwner.Name} ${dataItem.ObjectOwner.LastName}` : "",
                        hidden: false,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "OrganizationRights.Role", title: "Roller", width: 150,
                        persistId: "role", // DON'T YOU DARE RENAME!
                        attributes: { "class": "might-overflow" },
                        template: this.roleTemplate,
                        excelTemplate: this.roleTemplate,
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "isLocalAdmin", title: "Lokal Admin", width: 96,
                        persistId: "localadminrole", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        template: (dataItem) => dataItem.isLocalAdmin ? `<span class="glyphicon glyphicon-check text-success" aria-hidden="true"></span>` : `<span class="glyphicon glyphicon-unchecked" aria-hidden="true"></span>`,
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "isOrgAdmin", title: "Organisations Admin", width: 104,
                        persistId: "orgadminrole", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        template: (dataItem) => dataItem.isOrgAdmin ? `<span class="glyphicon glyphicon-check text-success" aria-hidden="true"></span>` : `<span class="glyphicon glyphicon-unchecked" aria-hidden="true"></span>`,
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "isProjectAdmin", title: "Projekt Admin", width: 109,
                        persistId: "projectadminrole", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        template: (dataItem) => dataItem.isProjectAdmin ? `<span class="glyphicon glyphicon-check text-success" aria-hidden="true"></span>` : `<span class="glyphicon glyphicon-unchecked" aria-hidden="true"></span>`,
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "isSystemAdmin", title: "System Admin", width: 104,
                        persistId: "systemadminrole", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        template: (dataItem) => dataItem.isSystemAdmin ? `<span class="glyphicon glyphicon-check text-success" aria-hidden="true"></span>` : `<span class="glyphicon glyphicon-unchecked" aria-hidden="true"></span>`,
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "isContractAdmin", title: "Kontrakt Admin", width: 112,
                        persistId: "contractadminrole", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        template: (dataItem) => dataItem.isContractAdmin ? `<span class="glyphicon glyphicon-check text-success" aria-hidden="true"></span>` : `<span class="glyphicon glyphicon-unchecked" aria-hidden="true"></span>`,
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "isReportAdmin", title: "Rapport Admin", width: 112,
                        persistId: "reportadminrole", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        template: (dataItem) => dataItem.isReportAdmin ? `<span class="glyphicon glyphicon-check text-success" aria-hidden="true"></span>` : `<span class="glyphicon glyphicon-unchecked" aria-hidden="true"></span>`,
                        hidden: false,
                        filterable: false,
                        sortable: false
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
        }

        private roleTemplate = (dataItem: IGridModel) => {
            var roleNames = this._.map(dataItem.OrganizationRights, "Role");
            this._.forEach(roleNames, (roleName, index) => {
                switch (roleName) {
                    case Models.OrganizationRole.LocalAdmin: roleNames[index] = "Lokal Admin"; break;
                    case Models.OrganizationRole.OrganizationModuleAdmin: roleNames[index] = "Organisations Admin"; break;
                    case Models.OrganizationRole.ProjectModuleAdmin: roleNames[index] = "Projekt Admin"; break;
                    case Models.OrganizationRole.SystemModuleAdmin: roleNames[index] = "System Admin"; break;
                    case Models.OrganizationRole.ContractModuleAdmin: roleNames[index] = "Kontrakt Admin"; break;
                    case Models.OrganizationRole.ReportModuleAdmin: roleNames[index] = "Rapport Admin"; break;
                }
            });
            return roleNames.join(",");
        }

        private fixNameFilter(filterUrl, column) {
            const pattern = new RegExp(`(\\w+\\()${column}(.*?\\))`, "i");
            if (column == 'ObjectOwner.Name') {
                return filterUrl.replace(pattern, `$1concat(concat(ObjectOwner/Name, ' '), ObjectOwner/LastName)$2`);
            }
            return filterUrl.replace(pattern, `$1concat(concat(Name, ' '), LastName)$2`);
        }

        private onEdit = (e: JQueryEventObject) => {
            e.preventDefault();
            var dataItem = this.mainGrid.dataItem(this.$(e.currentTarget).closest("tr"));
            var entityId = dataItem["Id"];
            this.$state.go("organization.user.edit", { id: entityId });
        }

        private onDelete = (e: JQueryEventObject) => {
            if (this.hasWriteAccess == true) {
                e.preventDefault();
                var dataItem = this.mainGrid.dataItem(this.$(e.currentTarget).closest("tr"));
                var entityId = dataItem["Id"];
                this.$state.go("organization.user.delete", { id: entityId });
                //this.mainGrid.dataSource.remove(dataItem);
                //this.mainGrid.dataSource.sync();
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
        .config(["$stateProvider", ($stateProvider) => {
            $stateProvider.state("organization.user", {
                url: "/user",
                templateUrl: "app/components/org/user/org-user.view.html",
                controller: OrganizationUserController,
                controllerAs: "ctrl",
                resolve: {
                    user: [
                        "userService", userService => userService.getUser()
                    ],
                    hasWriteAccess: [
                        '$http', '$stateParams', 'user', function ($http, $stateParams, user) {
                            return $http.get('api/Organization/' + user.currentOrganizationId + "?hasWriteAccess=true&organizationId=" + user.currentOrganizationId)
                                .then(function (result) {
                                    return result.data.response;
                                });
                        }
                    ]
                }
            });
        }
    ]);
}
