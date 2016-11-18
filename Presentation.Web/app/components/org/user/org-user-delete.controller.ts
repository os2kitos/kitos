module Kitos.Organization.Users {
    "use strict";

    interface IDeleteViewModel {
        isLocalAdmin: boolean;
        isOrgAdmin: boolean;
        isProjectAdmin: boolean;
        isSystemAdmin: boolean;
        isContractAdmin: boolean;
        isReportAdmin: boolean;
    }

    class UserRole {
        public modul: any;
        public rightId: any;
        public user: any;
        public userId: number;
    }

    class Map<T> {
        public items: { [key: string]: T };

        constructor() {
            this.items = {};
        }

        add(key: string, value: T): void {
            this.items[key] = value;
        }

        has(key: string): boolean {
            return key in this.items;
        }

        get(key: string): T {
            return this.items[key];
        }

        del(key: string): void {
            delete this.items[key];
        }
    }

    //Controller til at vise en brugers roller i en organisation
    class DeleteOrganizationUserController {
        public vm: IDeleteViewModel;
        
        public orgRoles: Map<UserRole> = new Map<UserRole>();
        public projectRoles: Map<UserRole> = new Map<UserRole>();
        public systemRoles: Map<UserRole> = new Map<UserRole>();
        public contractRoles: Map<UserRole> = new Map<UserRole>();
        
        public vmProject: any;
        public vmSystem: any;
        public vmItContracts: any;
        public vmGetUsers: any;
        public vmOrgUnits: any;
        public vmHasAdminRoles: boolean;
        public vmUsersInOrganization: any;
        public selecterUser: any;
        public isLocalAdminRoles: any;
        public isOrgAdminRoles: any;
        public isProjectAdminRoles: any;
        public isSystemAdminRoles: any;
        public isContractAdminRoles: any;
        public isReportAdminRoles: any;


        private userId: number;
        private firstName: string;
        private lastName: string;
        private email: string;
        private itemSelected: boolean;
        private originalVm;

        // injecter resolve request i ctoren
        public static $inject: string[] = [
            "$uibModalInstance",
            "$http",
            "$q",
            "$state",
            "notify",
            "user",
            "currentUser",
            "usersInOrganization",
            "projects",
            "system",
            "itContracts",
            "orgUnits",
            "_"];

        constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private $http: IHttpServiceWithCustomConfig,
            private $q: ng.IQService,
            private $state: ng.ui.IStateService,
            private notify,
            private user: Models.IUser,
            private currentUser: Services.IUser,
            private usersInOrganization: Models.IUser,
            public projects,
            public system,
            public itContracts,
            public orgUnits,
            private _: ILoDashWithMixins) {

            this.userId = user.Id;
            this.firstName = user.Name;
            this.lastName = user.LastName;
            this.email = user.Email;

            this.vmUsersInOrganization = usersInOrganization;
            this.vmProject = projects;
            this.vmSystem = system;
            this.vmItContracts = itContracts;
            this.vmOrgUnits = orgUnits;
            this.itemSelected = false;

            //tjekker om brugeren har de forskellige administrator rettigheder.
            var userVm: IDeleteViewModel = {
                isLocalAdmin: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.LocalAdmin }) !== undefined,
                isOrgAdmin: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.OrganizationModuleAdmin }) !== undefined,
                isProjectAdmin: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.ProjectModuleAdmin }) !== undefined,
                isSystemAdmin: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.SystemModuleAdmin }) !== undefined,
                isContractAdmin: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.ContractModuleAdmin }) !== undefined,
                isReportAdmin: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.ReportModuleAdmin }) !== undefined
            };
            this.vm = userVm;
            //tjekker om bruger har nogen admin rolle
            this.vmHasAdminRoles = userVm.isContractAdmin ||
                userVm.isLocalAdmin ||
                userVm.isOrgAdmin ||
                userVm.isProjectAdmin ||
                userVm.isReportAdmin ||
                userVm.isSystemAdmin;
            console.log(user.OrganizationRights);
        }


        public checkBoxTrueValue = (item) => {
            if (item == null) {
            } else {
                this.itemSelected = item;
            }
        }

        public collectionUpdate = (module, object, isChecked, selectedUser) => {
            if (selectedUser == null) {
            } else {
                if (isChecked) {
                    var data = JSON.parse(selectedUser);
                    var userRoles: UserRole = {
                        modul: module,
                        rightId: object.Id,
                        user: data.Name + " " + data.LastName,
                        userId: data.Id
                    }
                    if (module === "Organisation") {
                        this.orgRoles.add(object.Id, userRoles);
                    }
                    if (module === "Project") {
                        this.projectRoles.add(object.Id, userRoles);
                    }
                    if (module === "System") {
                        this.systemRoles.add(object.Id, userRoles);
                    }
                    if (module === "Contract") {
                        this.contractRoles.add(object.Id, userRoles);
                    }
                }
                if (!isChecked) {
                    if (module === "Organisation") {
                        this.orgRoles.del(object.Id);
                    }
                    if (module === "Project") {
                        this.projectRoles.del(object.Id);
                    }
                    if (module === "System") {
                        this.systemRoles.del(object.Id);
                    }
                    if (module === "Contract") {
                        this.contractRoles.del(object.Id);
                    }
                } 
            }
        }

        public setSelectedUser = (item) => {
            if (item == null) {

            } else {
                var data = JSON.parse(item);
                this.selecterUser = data;
            }
        }
        
        public collectionAdminUpdate = (module, isChecked, selectedUser) => {
            if ("LocalAdmin" === module) {
                if (isChecked) {
                    var localAdmin = {
                        modul: module,
                        rightId: this.user.Id,
                        userId: selectedUser.Id
                    };
                    this.isLocalAdminRoles = localAdmin;
                } else {
                    this.isLocalAdminRoles = null;
                }
            }
            if ("OrganizationModuleAdmin" === module) {
                if (isChecked) {
                    var orgAdmin = {
                        modul: module,
                        rightId: this.user.Id,
                        userId: selectedUser.Id
                    };
                    this.isOrgAdminRoles = orgAdmin;
                } else {
                    this.isOrgAdminRoles = null;
                }
            }
            if ("ProjectModuleAdmin" === module) {
                if (isChecked) {
                    var projectAdmin = {
                        modul: module,
                        rightId: this.user.Id,
                        userId: selectedUser.Id
                    };
                    this.isProjectAdminRoles = projectAdmin;
                } else {
                    this.isProjectAdminRoles = null;
                }
            }
            if ("SystemModuleAdmin" === module) {
                if (isChecked) {
                    var systemAdmin = {
                        modul: module,
                        rightId: this.user.Id,
                        userId: selectedUser.Id
                    };
                    this.isSystemAdminRoles = systemAdmin;
                } else {
                    this.isSystemAdminRoles = null;
                }
            }
            if ("ContractModuleAdmin" === module) {
                if (isChecked) {
                    var contractAdmin = {
                        modul: module,
                        rightId: this.user.Id,
                        userId: selectedUser.Id
                    };
                    this.isSystemAdminRoles = contractAdmin;
                } else {
                    this.isSystemAdminRoles = null;
                }
            }
            if ("ReportModuleAdmin" === module) {
                if (isChecked) {
                    var reportAdmin = {
                        modul: module,
                        rightId: this.user.Id,
                        userId: selectedUser.Id
                    };
                    this.isSystemAdminRoles = reportAdmin;
                } else {
                    this.isSystemAdminRoles = null;
                }
            }
        }

        public patchData() {
            var orgRoles = this.orgRoles;
            var projRoles = this.projectRoles;
            var sysRoles = this.systemRoles;
            var contRoles = this.contractRoles;

            if (orgRoles != null) {
                _.each(orgRoles.items,
                    (value, key) => {
                        let payload = {
                            UserId: value.userId
                        }
                        this.$http.patch(`/odata/OrganizationUnitRights(${value.rightId})`, payload);
                    });
            }

            if (projRoles != null) {
                _.each(projRoles.items,
                    (value, key) => {
                        let payload = {
                            UserId: value.userId
                        }
                        this.$http.patch(`/odata/ItProjectRights(${value.rightId})`, payload);
                    });
            }

            if (sysRoles != null) {
                _.each(sysRoles.items,
                    (value, key) => {
                        let payload = {
                            UserId: value.userId
                        }
                        this.$http.patch(`/odata/ItSystemRights(${value.rightId})`, payload);
                    });
            }

            if (contRoles != null) {
                _.each(contRoles.items,
                    (value, key) => {
                        let payload = {
                            UserId: value.userId
                        }
                        this.$http.patch(`/odata/ItContractRights(${value.rightId})`, payload);
                    });
            }
        }

        public patchAdminData() {
            if (this.isLocalAdminRoles != null) {
                    let payload = {
                        UserId: this.isLocalAdminRoles.userId
                    }
                    this.$http.patch(`/odata/OrganizationRights$filter=UserId eq ${this.isLocalAdminRoles.rightId} and Role eq ${this.isLocalAdminRoles.modul}`, payload);
            } 
            if (this.isOrgAdminRoles != null) {
                let payload = {
                    UserId: this.isOrgAdminRoles.userId
                }
                this.$http.patch(`/odata/OrganizationRights$filter=UserId eq ${this.isOrgAdminRoles.rightId} and Role eq ${this.isOrgAdminRoles.modul}`, payload);
            } 
            if (this.isProjectAdminRoles != null) {
                let payload = {
                    UserId: this.isProjectAdminRoles.userId
                }
                this.$http.patch(`/odata/OrganizationRights$filter=UserId eq ${this.isProjectAdminRoles.rightId} and Role eq ${this.isProjectAdminRoles.modul}`, payload);
            } 
            if (this.isSystemAdminRoles != null) {
                let payload = {
                    UserId: this.isSystemAdminRoles.userId
                }
                this.$http.patch(`/odata/OrganizationRights$filter=UserId eq ${this.isSystemAdminRoles.rightId} and Role eq ${this.isSystemAdminRoles.modul}`, payload);
            } 
            if (this.isContractAdminRoles != null) {
                let payload = {
                    UserId: this.isContractAdminRoles.userId
                }
                this.$http.patch(`/odata/OrganizationRights$filter=UserId eq ${this.isContractAdminRoles.rightId} and Role eq ${this.isContractAdminRoles.modul}`, payload);
            } 
            if (this.isReportAdminRoles != null) {
                let payload = {
                    UserId: this.isReportAdminRoles.userId
                }
                this.$http.patch(`/odata/OrganizationRights?$filter=UserId eq ${this.isReportAdminRoles.rightId} and Role eq ${this.isReportAdminRoles.modul}`, payload);
            } 
        }

        public ok() {
            this.patchData();
            this.patchAdminData();
            this.$uibModalInstance.close();
        }

        public assign() {
            this.patchData();
            this.patchAdminData();
            this.$uibModalInstance.close(this.$state.reload());
        }

        public cancel() {
            this.$uibModalInstance.dismiss();
        }

    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("organization.user.delete", {
                url: "/:id/delete",
                onEnter: [
                    "$state", "$stateParams", "$uibModal",
                    ($state: ng.ui.IStateService, $stateParams: ng.ui.IStateParamsService, $uibModal: ng.ui.bootstrap.IModalService) => {
                        $uibModal.open({
                            templateUrl: "app/components/org/user/org-user-delete.modal.view.html",
                            windowClass: "modal fade in",
                            controller: DeleteOrganizationUserController,
                            controllerAs: "ctrl",
                            resolve: {
                                currentUser: [
                                    "userService",
                                    (userService) => userService.getUser()
                                ],
                                user: [
                                    "$http", "userService",
                                    ($http: ng.IHttpService, userService) =>
                                        userService.getUser()
                                            .then((currentUser) => $http
                                                .get(`/odata/Users(${$stateParams["id"]})?$expand=OrganizationRights($filter=OrganizationId eq ${currentUser.currentOrganizationId})`)
                                                .then(result => result.data))
                                ],
                                usersInOrganization: [
                                    "$http", "userService", "UserGetService",
                                    ($http: ng.IHttpService, userService, userGetService) =>
                                        userService.getUser()
                                            .then((currentUser) => userGetService.GetAllUsersFromOrganizationById(`${currentUser.currentOrganizationId}`)
                                                .then(result => result.data.value))
                                ],
                                //Henter data for de forskellige collections ved brug er servicen "ItProjectService"
                                projects: ["ItProjectService", (itProjects) =>
                                    itProjects.GetProjectDataById($stateParams["id"])
                                        .then(projectResult => projectResult.data.value)
                                ],
                                //Henter data for de forskellige collections ved brug er servicen "ItSystemService"
                                system: ["ItSystemService", (itSystems) =>
                                    itSystems.GetSystemDataByIdFiltered($stateParams["id"])
                                        .then(systemResult => systemResult.data.value)
                                ],
                                //Henter data for de forskellige collections ved brug er servicen "ItContractService"
                                itContracts: ["ItContractsService", (itContracts) =>
                                    itContracts.GetContractDataById($stateParams["id"])
                                        .then(systemResult => systemResult.data.value)
                                ],
                                //Henter data for de forskellige collections ved brug er servicen "OrganizationService"
                                orgUnits: ["organizationService", (orgUnits) =>
                                    orgUnits.GetOrganizationUnitDataById($stateParams["id"])
                                        .then(result => result.data.value)
                                ]
                            }
                        })
                            .result.then(() => {
                                // OK
                                // GOTO parent state and reload
                                $state.go("^", null, { reload: true });
                            },
                            () => {
                                // Cancel
                                // GOTO parent state
                                $state.go("^");
                            });
                    }
                ]
            });
        }]);
}
