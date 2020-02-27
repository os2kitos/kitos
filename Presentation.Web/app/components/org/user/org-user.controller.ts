module Kitos.Organization.Users {
    "use strict";

    interface  IGridModel extends Models.IUser {
        hasApi: boolean;
        canEdit: boolean;
        isLocalAdmin: boolean;
        isOrgAdmin: boolean;
        isProjectAdmin: boolean;
        isSystemAdmin: boolean;
        isContractAdmin: boolean;
        isReportAdmin: boolean;
    }


    export interface IOverviewController {
        mainGrid: IKendoGrid<IGridModel>;
        mainGridOptions: IKendoGridOptions<IGridModel>;
    }

    export class OrganizationUserController implements IOverviewController {

        private storageKey = "org-overview";
        private gridState = this.gridStateService.getService(this.storageKey, this.user.id);
        public mainGrid: IKendoGrid<IGridModel>;
        public mainGridOptions: IKendoGridOptions<IGridModel>;

        public static $inject: Array<string> = [
            "$rootScope",
            "$scope",
            "$http",
            "$timeout",
            "$state",
            "_",
            "$",
            "user",
            "hasWriteAccess",
            "notify",
            "gridStateService",
            "exportGridToExcelService"
        ];

        constructor(
            private $rootScope: IRootScope,
            private $scope: ng.IScope,
            private $http: ng.IHttpService,
            private $timeout: ng.ITimeoutService,
            private $state: ng.ui.IStateService,
            private _: ILoDashWithMixins,
            private $: JQueryStatic,
            private user,
            private hasWriteAccess,
            private notify,
            private gridStateService: Services.IGridStateFactory,
            private exportGridToExcelService) {
            this.hasWriteAccess = hasWriteAccess;
            $scope.$on("kendoWidgetCreated", (event, widget) => {
                if (widget === this.mainGrid) {
                    this.loadGridOptions();
                    this.mainGrid.dataSource.read();
                }
            });

            this.activate();
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
            var mainGridOptions: IKendoGridOptions<IGridModel> = {
                autoBind: false,
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: `/odata/Organizations(${this.user.currentOrganizationId})/Organizations.GetUsers`,
                            dataType: "json",
                            data: {
                                $expand: `ObjectOwner,OrganizationUnitRights($expand=Role($select=Name)),OrganizationRights($filter=OrganizationId eq ${this.user.currentOrganizationId})`
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
                toolbar: [
                    {
                        name: "clearFilter",
                        text: "Nulstil",
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
                    },
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
                height: window.innerHeight - 200,
                detailTemplate: (dataItem) => `<uib-tabset active="0">
                    <uib-tab index="0" heading="Organisation roller"><user-organization-unit-roles user-id="${dataItem.Id}" current-organization-id="${this.user.currentOrganizationId}"></user-organization-unit-roles></uib-tab>
                    <uib-tab index="1" heading="Projekt roller"><user-project-roles user-id="${dataItem.Id}" current-organization-id="${this.user.currentOrganizationId}"></user-project-roles></uib-tab>
                    <uib-tab index="2" heading="System roller"><user-system-roles user-id="${dataItem.Id}" current-organization-id="${this.user.currentOrganizationId}"></user-system-roles></uib-tab>
                    <uib-tab index="3" heading="Kontrakt roller"><user-contract-roles user-id="${dataItem.Id}" current-organization-id="${this.user.currentOrganizationId}"></user-contract-roles></uib-tab>
                </uib-tabset>`,
                dataBound: this.saveGridOptions,
                columnResize: this.saveGridOptions,
                columnHide: this.saveGridOptions,
                columnShow: this.saveGridOptions,
                columnReorder: this.saveGridOptions,
                excelExport: this.exportToExcel,
                page: this.onPaging,
                columns: [
                    {
                        field: "Name", title: "Navn", width: 230,
                        persistId: "fullname", // DON'T YOU DARE RENAME!
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
                        persistId: "email", // DON'T YOU DARE RENAME!
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
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "OrganizationUnitRights.Role", title: "Roller", width: 150,
                        persistId: "role", // DON'T YOU DARE RENAME!
                        attributes: { "class": "might-overflow" },
                        template: (dataItem) => {
                            this.curOrgCheck = dataItem.OrganizationUnitRights.ObjectId === this.user.currentOrganizationId;
                            return `<span data-ng-model="dataItem.OrganizationUnitRights" value="rights.Role.Name" ng-repeat="rights in dataItem.OrganizationUnitRights | filter: { ObjectId: '${this.user.currentOrganizationId}' }"> {{rights.Role.Name}}<span data-ng-if="projectOverviewVm.checkIfRoleIsAvailable(rights.Role.Id)">(udgået)</span>{{$last ? '' : ', '}}</span>`;
                        },
                        hidden: true,
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

                        field: "hasApi", title: "API adgang", width: 96,
                        persistId: "apiaccess", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center", "data-element-type": "userObject" },
                        headerAttributes: {
                            "data-element-type": "userHeader"
                        },
                        template: (dataItem) => setBooleanValue(dataItem.HasApiAccess),
                        hidden: !(this.user.isGlobalAdmin || this.user.isLocalAdmin),
                        filterable: false,
                        sortable: false,
                        menu: (this.user.isGlobalAdmin || this.user.isLocalAdmin),
                    },
                    {
                        field: "isLocalAdmin", title: "Lokal Admin", width: 96,
                        persistId: "localadminrole", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        template: (dataItem) => setBooleanValue(dataItem.isLocalAdmin),
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "isOrgAdmin", title: "Organisations Admin", width: 104,
                        persistId: "orgadminrole", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        template: (dataItem) => setBooleanValue(dataItem.isOrgAdmin),
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "isProjectAdmin", title: "Projekt Admin", width: 109,
                        persistId: "projectadminrole", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        template: (dataItem) => setBooleanValue(dataItem.isProjectAdmin),
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "isSystemAdmin", title: "System Admin", width: 104,
                        persistId: "systemadminrole", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        template: (dataItem) => setBooleanValue(dataItem.isSystemAdmin),
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "isContractAdmin", title: "Kontrakt Admin", width: 112,
                        persistId: "contractadminrole", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        template: (dataItem) => setBooleanValue(dataItem.isContractAdmin),
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "isReportAdmin", title: "Rapport Admin", width: 112,
                        persistId: "reportadminrole", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        template: (dataItem) => setBooleanValue(dataItem.isReportAdmin),
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        template: (dataItem) => dataItem.canEdit ? `<a data-ng-click="ctrl.onEdit(${dataItem.Id})" class="k-button k-button-icontext"><span class="k-icon k-edit"></span>Redigér</a><a data-ng-click="ctrl.onDelete(${dataItem.Id})" class="k-button k-button-icontext" data-user="dataItem"><span class="k-icon k-delete"></span>Slet</a>` : `<a class="k-button k-button-icontext" data-ng-disabled="${!dataItem.canEdit}"><span class="k-icon k-edit"></span>Redigér</a><a class="k-button k-button-icontext" data-user="dataItem" data-ng-disabled="${!dataItem.canEdit}"><span class="k-icon k-delete"></span>Slet</a>`,
                        title: " ",
                        width: 176,
                        persistId: "command"
                    }
                ]
            };

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

        public onEdit(entityId) {
            this.$state.go("organization.user.edit", { id: entityId });
        }

        private fixNameFilter(filterUrl, column) {
            const pattern = new RegExp(`(\\w+\\()${column}(.*?\\))`, "i");
            if (column == 'ObjectOwner.Name') {
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

        private exportToExcel = (e: IKendoGridExcelExportEvent<Models.IOrganizationRight>) => {
            this.exportGridToExcelService.getExcel(e, this._, this.$timeout, this.mainGrid);
        }

        public curOrgCheck: boolean;
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
                    ]
                }
            });
        }
    ]);
}
