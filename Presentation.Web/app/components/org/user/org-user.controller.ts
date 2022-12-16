module Kitos.Organization.Users {
    "use strict";

    interface IGridModel extends Models.IUser {
        hasApi: boolean;
        canEdit: boolean;
        isLocalAdmin: boolean;
        isOrgAdmin: boolean;
        isSystemAdmin: boolean;
        isContractAdmin: boolean;
        isRightsHolder: boolean;
    }


    export interface IOverviewController {
        mainGrid: IKendoGrid<IGridModel>;
        mainGridOptions: IKendoGridOptions<IGridModel>;
    }

    export class OrganizationUserController implements IOverviewController {

        private storageKey = "org-overview";
        private gridState = this.gridStateService.getService(this.storageKey, this.user);
        public mainGrid: IKendoGrid<IGridModel>;
        public mainGridOptions: IKendoGridOptions<IGridModel>;

        public static $inject: Array<string> = [
            "$scope",
            "$state",
            "_",
            "user",
            "hasWriteAccess",
            "notify",
            "gridStateService",
            "exportGridToExcelService",
            "$timeout",
            "adminPermissions"
        ];

        constructor(
            private readonly $scope: ng.IScope,
            private readonly $state: ng.ui.IStateService,
            private readonly _: ILoDashWithMixins,
            private readonly user,
            private readonly hasWriteAccess,
            private readonly notify,
            private readonly gridStateService: Services.IGridStateFactory,
            private readonly exportGridToExcelService: Services.System.ExportGridToExcelService,
            private readonly $timeout: ng.ITimeoutService,
            private readonly adminPermissions: Models.Users.UserAdministrationPermissionsDTO) {
            $scope.$on("kendoWidgetCreated", (event, widget) => {
                if (widget === this.mainGrid) {
                    this.loadGridOptions();
                }
            });

            //Defer loading grid unitil navigtion completed
            setTimeout(() => this.activate(), 1);
        }

        private hasRole(user: IGridModel, role: Models.OrganizationRole): boolean {
            return this._.find(user.OrganizationRights, (right) => right.Role === role) !== undefined;
        }

        private saveGridOptions = () => {
            this.gridState.saveGridOptions(this.mainGrid);
        }

        private onPaging = () => {
            Utility.KendoGrid.KendoGridScrollbarHelper.resetScrollbarPosition(this.mainGrid);
        }

        private loadGridOptions() {
            this.gridState.loadGridOptions(this.mainGrid);
        }

        public saveGridProfile() {
            this.gridState.saveGridProfile(this.mainGrid);
            this.notify.addSuccessMessage("Filtre og sortering gemt");
        }

        public loadGridProfile() {
            this.gridState.loadGridProfile(this.mainGrid);
            this.mainGrid.dataSource.read();
            this.notify.addSuccessMessage("Anvender gemte filtre og sortering");
        }

        public clearGridProfile() {
            this.gridState.removeProfile();
            this.gridState.removeSession();
            this.notify.addSuccessMessage("Filtre og sortering slettet");
            this.reload();
        }

        public doesGridProfileExist() {
            return this.gridState.doesGridProfileExist();
        }

        public clearOptions() {
            this.gridState.removeProfile();
            this.gridState.removeLocal();
            this.gridState.removeSession();
            this.notify.addSuccessMessage("Sortering, filtering og kolonnevisning, -bredde og –rækkefølge nulstillet");
            this.reload();
        };

        public generateExcel() {
            kendo.ui.progress(this.mainGrid.element, true);
        }

        private reload() {
            this.$state.go(".", null, { reload: true });
        }

        private activate() {
            const allowDelete = this.adminPermissions.allowDelete;
            var mainGridOptions: IKendoGridOptions<IGridModel> = {
                autoBind: false,
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: `/odata/Organizations(${this.user.currentOrganizationId})/Organizations.GetUsers`,
                            dataType: "json",
                            data: {
                                $expand: `ObjectOwner,OrganizationUnitRights($filter=ObjectId eq ${this.user.currentOrganizationId}; $expand=Role($select=Name)),OrganizationRights($filter=OrganizationId eq ${this.user.currentOrganizationId})`
                            }
                        },
                        destroy: {
                            url: () => {
                                return `/odata/Organizations(${this.user.currentOrganizationId})/RemoveUser()`;
                            },
                            dataType: "json",
                            contentType: "application/json"
                        },
                        parameterMap: (options, operation) => {
                            if (operation === "read") {
                                // get kendo to map parameters to an odata url
                                const parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, operation);
                                if (parameterMap.$filter) {
                                    parameterMap.$filter = this.fixNameFilter(parameterMap.$filter, "Name");
                                    parameterMap.$filter = this.fixNameFilter(parameterMap.$filter, "ObjectOwner.Name");
                                }
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
                                LastAdvisDate: { type: "date" }
                            }
                        },
                        parse: response => {
                            // iterate each user
                            this._.forEach(response.value, (usr: IGridModel) => {
                                usr.canEdit = this.hasWriteAccess;
                                // remove the user role
                                this._.remove(usr.OrganizationRights, (right) => right.Role === Models.OrganizationRole.User);

                                usr.isLocalAdmin = this.hasRole(usr, Models.OrganizationRole.LocalAdmin);
                                usr.isOrgAdmin = this.hasRole(usr, Models.OrganizationRole.OrganizationModuleAdmin);
                                usr.isSystemAdmin = this.hasRole(usr, Models.OrganizationRole.SystemModuleAdmin);
                                usr.isContractAdmin = this.hasRole(usr, Models.OrganizationRole.ContractModuleAdmin);
                                usr.isRightsHolder = this.hasRole(usr, Models.OrganizationRole.RightsHolderAccess);
                                usr.ObjectOwner ??= { Name: "", LastName: "" } as any;
                            });
                            return response;
                        }
                    }
                } as kendo.data.DataSourceOptions,
                toolbar: [
                    {
                        name: "clearFilter",
                        text: "Gendan kolonneopsætning",
                        template: "<button type='button' class='k-button k-button-icontext' title='Nulstil sortering, filtering og kolonnevisning, -bredde og –rækkefølge' data-ng-click='ctrl.clearOptions()' data-element-type='resetFilterButton'>#: text #</button>"
                    },
                    {
                        name: "saveFilter",
                        text: "Gem filter",
                        template: "<button type='button' class='k-button k-button-icontext' title='Gem filtre og sortering' data-ng-click='ctrl.saveGridProfile()' data-element-type='saveFilterButton'>#: text #</button>"
                    },
                    {
                        name: "useFilter",
                        text: "Anvend filter",
                        template: "<button type='button' class='k-button k-button-icontext' title='Anvend gemte filtre og sortering' data-ng-click='ctrl.loadGridProfile()' data-ng-disabled='!systemOverviewVm.doesGridProfileExist()' data-element-type='useFilterButton'>#: text #</button>"
                    },
                    {
                        name: "deleteFilter",
                        text: "Slet filter",
                        template: "<button type='button' class='k-button k-button-icontext' title='Slet filtre og sortering' data-ng-click='ctrl.clearGridProfile()' data-ng-disabled='!systemOverviewVm.doesGridProfileExist()' data-element-type='removeFilterButton'>#: text #</button>"
                    }
                ],
                excel: {
                    fileName: "Brugere.xlsx",
                    filterable: true,
                    allPages: true,
                },
                pageable: {
                    refresh: true,
                    pageSizes: [10, 25, 50, 100, 200, "all"],
                    buttonCount: 5
                },
                sortable: {
                    mode: "single"
                },
                editable: false,
                reorderable: true,
                resizable: true,
                filterable: {
                    mode: "row"
                },
                groupable: false,
                columnMenu: true,
                excelExport: (e: any) => this.exportToExcel(e),
                height: window.innerHeight - 200,
                detailTemplate: (dataItem) => `<uib-tabset active="0">
                    <uib-tab index="0" heading="Organisation roller"><user-organization-unit-roles user-id="${dataItem.Id}" current-organization-id="${this.user.currentOrganizationId}"></user-organization-unit-roles></uib-tab>
                    <uib-tab index="2" heading="System roller"><user-system-roles user-id="${dataItem.Id}" current-organization-id="${this.user.currentOrganizationId}"></user-system-roles></uib-tab>
                    <uib-tab index="3" heading="Kontrakt roller"><user-contract-roles user-id="${dataItem.Id}" current-organization-id="${this.user.currentOrganizationId}"></user-contract-roles></uib-tab>
                    <uib-tab index="4" heading="Databehandlingsroller"><user-data-processing-registration-roles user-id="${dataItem.Id}" current-organization-id="${this.user.currentOrganizationId}"></user-data-processing-registration-roles></uib-tab>
                </uib-tabset>`,
                dataBound: this.saveGridOptions,
                columnResize: this.saveGridOptions,
                columnHide: this.saveGridOptions,
                columnShow: this.saveGridOptions,
                columnReorder: this.saveGridOptions,
                page: this.onPaging,
                columns: [
                    {
                        field: "Name", title: "Navn", width: 230,
                        persistId: "fullname",
                        template: (dataItem) => `${dataItem.Name} ${dataItem.LastName}`,
                        excelTemplate: (dataItem) => `${dataItem.Name} ${dataItem.LastName}`,
                        hidden: false,
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
                        field: "Email", title: "Email", width: 230,
                        persistId: "email",
                        template: (dataItem) => `${dataItem.Email}`,
                        excelTemplate: (dataItem) => dataItem.Email,
                        headerAttributes: {
                            "data-element-type": "userHeaderEmail"
                        },
                        attributes: {
                            "data-element-type": "userEmailObject"
                        },
                        hidden: false,
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
                        field: "LastAdvisDate", title: "Advis", width: 110,
                        persistId: "advisdate",
                        template: (dataItem) => `<advis-button data-user="dataItem" data-current-organization-id="${this.user.currentOrganizationId}" data-ng-disabled="${!dataItem.canEdit}"></advis>`,
                        excelTemplate: (dataItem) => dataItem.LastAdvisDate ? Kitos.Helpers.ExcelExportHelper.renderDate(dataItem.LastAdvisDate) : "",
                        hidden: false,
                        filterable: false
                    },
                    {
                        field: "ObjectOwner.Name", title: "Oprettet af", width: 150,
                        persistId: "createdby",
                        template: (dataItem) => dataItem.ObjectOwner ? `${dataItem.ObjectOwner.Name} ${dataItem.ObjectOwner.LastName}` : "",
                        excelTemplate: (dataItem) => dataItem.ObjectOwner ? `${dataItem.ObjectOwner.Name} ${dataItem.ObjectOwner.LastName}` : "",
                        hidden: false,
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
                        field: "OrganizationUnitRights.Role",
                        title: "Organisationsroller",
                        width: 150,
                        filterable: false,
                        sortable: false,
                        persistId: "role",
                        attributes: { "class": "might-overflow" },
                        template: (dataItem) => {
                            if (dataItem.OrganizationUnitRights.length == 0) {
                                return "";
                            }
                            return `<span data-ng-model="dataItem.OrganizationUnitRights" value="rights.Role.Name" ng-repeat="rights in dataItem.OrganizationUnitRights"> {{rights.Role.Name}}{{$last ? '' : ', '}}</span>`;
                        },
                        excelTemplate: (dataItem) => dataItem.OrganizationUnitRights.map(right => right.Role.Name).join(", "),
                        hidden: true
                    },
                    {
                        field: "hasApi", title: "API bruger", width: 96,
                        persistId: "apiaccess",
                        attributes: { "class": "text-center", "data-element-type": "userObject" },
                        headerAttributes: {
                            "data-element-type": "userHeader"
                        },
                        template: (dataItem) => setBooleanValue(dataItem.HasApiAccess),
                        excelTemplate: (dataItem) => Kitos.Helpers.ExcelExportHelper.renderBoolean(dataItem.HasApiAccess),
                        hidden: !(this.user.isGlobalAdmin || this.user.isLocalAdmin),
                        filterable: false,
                        sortable: false,
                        menu: (this.user.isGlobalAdmin || this.user.isLocalAdmin),
                    },
                    {
                        field: "isLocalAdmin", title: "Lokal Admin", width: 96,
                        persistId: "localadminrole",
                        attributes: { "class": "text-center" },
                        template: (dataItem) => setBooleanValue(dataItem.isLocalAdmin),
                        excelTemplate: (dataItem) => Kitos.Helpers.ExcelExportHelper.renderBoolean(dataItem.isLocalAdmin),
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "isOrgAdmin", title: "Organisations Admin", width: 104,
                        persistId: "orgadminrole",
                        attributes: { "class": "text-center" },
                        template: (dataItem) => setBooleanValue(dataItem.isOrgAdmin),
                        excelTemplate: (dataItem) => Kitos.Helpers.ExcelExportHelper.renderBoolean(dataItem.isOrgAdmin),
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "isSystemAdmin", title: "System Admin", width: 104,
                        persistId: "systemadminrole",
                        attributes: { "class": "text-center" },
                        template: (dataItem) => setBooleanValue(dataItem.isSystemAdmin),
                        excelTemplate: (dataItem) => Kitos.Helpers.ExcelExportHelper.renderBoolean(dataItem.isSystemAdmin),
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "isContractAdmin", title: "Kontrakt Admin", width: 112,
                        persistId: "contractadminrole",
                        attributes: { "class": "text-center" },
                        template: (dataItem) => setBooleanValue(dataItem.isContractAdmin),
                        excelTemplate: (dataItem) => Kitos.Helpers.ExcelExportHelper.renderBoolean(dataItem.isContractAdmin),
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {

                        field: "rightsHolder", title: "Rettighedshaveradgang", width: 160,
                        persistId: "rightsHolder",
                        attributes: { "class": "text-center", "data-element-type": "rightsHolderObject" },
                        headerAttributes: {
                            "data-element-type": "rightsHolderHeader"
                        },
                        template: (dataItem) => setBooleanValue(dataItem.isRightsHolder),
                        excelTemplate: (dataItem) => Kitos.Helpers.ExcelExportHelper.renderBoolean(dataItem.isRightsHolder),
                        hidden: !this.user.isGlobalAdmin,
                        filterable: false,
                        sortable: false,
                        menu: this.user.isGlobalAdmin,
                    },
                    {

                        field: "stakeHolder", title: "Interessentadgang", width: 160,
                        persistId: "stakeHolder",
                        attributes: { "class": "text-center", "data-element-type": "stakeHolderObject" },
                        headerAttributes: {
                            "data-element-type": "stakeHolderHeader"
                        },
                        template: (dataItem) => setBooleanValue(dataItem.HasStakeHolderAccess),
                        excelTemplate: (dataItem) => Kitos.Helpers.ExcelExportHelper.renderBoolean(dataItem.HasStakeHolderAccess),
                        hidden: !this.user.isGlobalAdmin,
                        filterable: false,
                        sortable: false,
                        menu: this.user.isGlobalAdmin,
                    },
                    {
                        template: (dataItem) => `<a data-ng-click="ctrl.onEdit(${dataItem.Id})" ng-if="${dataItem.canEdit}" class="k-button k-button-icontext"><span class="k-icon k-edit"></span>Redigér</a><a data-ng-click="ctrl.onDelete(${dataItem.Id})" ng-if="${allowDelete}" class="k-button k-button-icontext" data-user="dataItem"><span class="k-icon k-delete"></span>Slet</a>`,
                        field: "Name", //Must bind to something or it corrupts the excel outputs
                        title: " ",
                        filterable: false,
                        sortable: false,
                        menu: false,
                        width: 176,
                        persistId: "rowCommands",
                        uiOnlyColumn: true,
                        isAvailable: this.hasWriteAccess || allowDelete
                    }
                ]
            };

            Helpers.ExcelExportHelper.setupExcelExportDropdown(() => this.excelConfig,
                () => this.mainGrid,
                this.$scope,
                mainGridOptions.toolbar);

            function customFilter(args) {
                args.element.kendoAutoComplete({
                    noDataTemplate: ''
                });
            }

            function setBooleanValue(value) {
                return value
                    ? `<span class="glyphicon glyphicon-check text-success" aria-hidden="true"></span>`
                    : `<span class="glyphicon glyphicon-unchecked" aria-hidden="true"></span>`;
            }

            this.mainGridOptions = mainGridOptions;
        }

        //NOTE: Stores the visibility parameters, and is used by the excel dropdown commands before invoking exportToExcel()..
        private readonly excelConfig: Models.IExcelConfig = {
        };

        private exportToExcel = (e: IKendoGridExcelExportEvent<Models.ItSystem.IItSystem>) => {
            this.exportGridToExcelService.getExcel(e, this._, this.$timeout, this.mainGrid, this.excelConfig);
        }

        public onEdit(entityId) {
            this.$state.go("organization.user.edit", { id: entityId });
        }

        private fixNameFilter(filterUrl, column) {
            const pattern = new RegExp(`(\\w+\\()${column}(.*?\\))`, "i");
            if (column === 'ObjectOwner.Name') {
                return filterUrl.replace(pattern, `$1concat(concat(ObjectOwner/Name, ' '), ObjectOwner/LastName)$2`);
            }
            return filterUrl.replace(pattern, `$1concat(concat(Name, ' '), LastName)$2`);
        }

        public onDelete(entityId) {
            if (this.hasWriteAccess == true) {
                this.$state.go("organization.user.delete", { id: entityId });
            } else {
                this.notify.addErrorMessage("Brugeren har ikke rettigheder til at ændre i organisationen");
            }
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
                    userAccessRights: ["authorizationServiceFactory", "user",
                        (authorizationServiceFactory: Kitos.Services.Authorization.IAuthorizationServiceFactory, user) =>
                            authorizationServiceFactory
                                .createOrganizationAuthorization()
                                .getAuthorizationForItem(user.currentOrganizationId)
                    ],
                    hasWriteAccess: ["userAccessRights", userAccessRights => userAccessRights.canEdit
                    ],
                    adminPermissions: ["userService", "user", (userService: Services.IUserService, user: Services.IUser) => userService.getPermissions(user.currentOrganizationUuid)
                    ]
                }
            });
        }
        ]);
}
