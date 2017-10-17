module Kitos.Organization.Users {
    "use strict";

    class UserRole {
        public modul: any;
        public rightId: any;
        public objectId: any;
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
        public orgRoles: Map<UserRole> = new Map<UserRole>();
        public projectRoles: Map<UserRole> = new Map<UserRole>();
        public systemRoles: Map<UserRole> = new Map<UserRole>();
        public contractRoles: Map<UserRole> = new Map<UserRole>();
        public adminRoles: Map<UserRole> = new Map<UserRole>();
        public vmOrgRoles: Map<UserRole> = new Map<UserRole>();
        public vmProjectRoles: Map<UserRole> = new Map<UserRole>();
        public vmSystemRoles: Map<UserRole> = new Map<UserRole>();
        public vmContractRoles: Map<UserRole> = new Map<UserRole>();
        public vmAdminRoles: Map<UserRole> = new Map<UserRole>();

        public vmOrgUnits: any;
        public vmProject: any;
        public vmSystem: any;
        public vmItContracts: any;
        public vmGetUsers: any;
        public vmOrgAdmin: any;
        public vmUsersInOrganization: any;
        public selectedUser: any;
        public isUserSelected: boolean;
        public curOrganization: string;
        public dirty: boolean;
        public disabled: boolean;

        private userId: number;
        private firstName: string;
        private lastName: string;
        private email: string;
        private itemSelected: boolean;
        private vmText: string;

        // injecter resolve request i ctoren
        public static $inject: string[] = [
            "$uibModalInstance",
            "$http",
            "$q",
            "$state",
            "$scope",
            "notify",
            "user",
            "currentUser",
            "usersInOrganization",
            "projects",
            "system",
            "itContracts",
            "orgUnits",
            "orgAdmin",
            "_",
            "text"
            ];

        constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private $http: IHttpServiceWithCustomConfig,
            private $q: ng.IQService,
            private $state: ng.ui.IStateService,
            private $scope,
            private notify,
            private user: any,
            private currentUser: Services.IUser,
            private usersInOrganization: any,
            public projects,
            public system,
            public itContracts,
            public orgUnits,
            public orgAdmin,
            private _: ILoDashWithMixins,
            private text) {

            this.userId = user.Id;
            this.firstName = user.Name;
            this.lastName = user.LastName;
            this.email = user.Email;

            this.initCollections(orgUnits, this.vmOrgRoles);
            this.initCollections(projects, this.vmProjectRoles);
            this.initCollections(system, this.vmSystemRoles);
            this.initCollections(itContracts, this.vmContractRoles);
            this.initCollections(orgAdmin, this.vmAdminRoles);
      
            this.vmUsersInOrganization = usersInOrganization.filter(x => x.Id !== user.Id);
            this.vmProject = projects;
            this.vmSystem = system;
            this.vmItContracts = itContracts;
            this.vmOrgUnits = orgUnits;
            this.vmOrgAdmin = orgAdmin.filter(bar => (bar.Role !== "User")).filter(bar => (bar.Role !== "ReadOnly"));
            this.itemSelected = false;
            this.isUserSelected = true;
            this.curOrganization = user.currentOrganizationName;
            this.disabled = true;
            this.vmText = text;
        }

        public initCollections = (collection, output) => {
            for (var item of collection) {
                output.add(item.Id, item);
            }
        }

        public disableBtns(val) {
            this.disabled = val;
        }

        public collectionUpdate = (module, object, isChecked) => {
            this.disableBtns(this.isUserSelected);
            if (isChecked) {
                this.dirty = false;

                var userRoles: UserRole = {
                    modul: module,
                    rightId: object.Id,
                    objectId: object.ObjectId
                };

                if (module === "OrganizationUnitRights") {
                    this.orgRoles.add(object.Id, userRoles);
                }
                if (module === "ItProjectRights") {
                    this.projectRoles.add(object.Id, userRoles);
                }
                if (module === "ItSystemRights") {
                    this.systemRoles.add(object.Id, userRoles);
                }
                if (module === "ItContractRights") {
                    this.contractRoles.add(object.Id, userRoles);
                }
                if (module === "OrganizationRights") {
                    this.adminRoles.add(object.Id, userRoles);
                }
            }
            if (!isChecked) {
                if (module === "OrganizationUnitRights") {
                    this.orgRoles.del(object.Id);
                }
                if (module === "ItProjectRights") {
                    this.projectRoles.del(object.Id);
                }
                if (module === "ItSystemRights") {
                    this.systemRoles.del(object.Id);
                }
                if (module === "ItContractRights") {
                    this.contractRoles.del(object.Id);
                }
                if (module === "OrganizationRights") {
                    this.adminRoles.del(object.Id);
                }
            }
        }

        public setSelectedUser = () => {
            if (this.selectedUser == null) {
                this.isUserSelected = true;
            } else {
                this.isUserSelected = false;
                this.disableBtns(this.isUserSelected);
            }
        }

        public patchData() {
            var orgRoles = this.orgRoles;
            var projRoles = this.projectRoles;
            var sysRoles = this.systemRoles;
            var contRoles = this.contractRoles;
            var adminRoles = this.adminRoles;

            if (orgRoles != null) {
                _.each(orgRoles.items,
                    (value, key) => {
                        let payload = {
                            UserId: this.selectedUser.Id
                        }
                        this.$http.patch(`/odata/OrganizationUnitRights(${value.rightId})`, payload)
                            .then(() => this.orgRoles.del(value.rightId));
                        this.vmOrgUnits = this.vmOrgUnits.filter(bar => (bar.Id !== value.rightId));
                    });
            }

            if (projRoles != null) {
                _.each(projRoles.items,
                    (value, key) => {
                        let payload = {
                            UserId: this.selectedUser.Id
                        }
                        this.$http.patch(`/odata/ItProjectRights(${value.rightId})`, payload)
                            .then(() => this.projectRoles.del(value.rightId));
                        this.vmProject = this.vmProject.filter(bar => (bar.Id !== value.rightId));
                    });
            }

            if (sysRoles != null) {
                _.each(sysRoles.items,
                    (value, key) => {
                        let payload = {
                            UserId: this.selectedUser.Id
                        }
                        this.$http.patch(`/odata/ItSystemRights(${value.rightId})`, payload)
                            .then(() => this.systemRoles.del(value.rightId));
                        this.vmSystem = this.vmSystem.filter(bar => (bar.Id !== value.rightId));
                    });
            }

            if (contRoles != null) {
                _.each(contRoles.items,
                    (value, key) => {
                        let payload = {
                            UserId: this.selectedUser.Id
                        }
                        this.$http.patch(`/odata/ItContractRights(${value.rightId})`, payload)
                            .then(() => this.contractRoles.del(value.rightId));
                        this.vmItContracts = this.vmItContracts.filter(bar => (bar.Id !== value.rightId));
                    });
            }
            if (adminRoles != null) {
                _.each(adminRoles.items,
                    (value, key) => {
                        let payload = {
                            UserId: this.selectedUser.Id
                        }
                        this.$http.patch(`/odata/OrganizationRights(${value.rightId})`, payload)
                            .then(() => this.adminRoles.del(value.rightId));
                        this.vmOrgAdmin = this.vmOrgAdmin.filter(bar => (bar.Id !== value.rightId));
                        this.orgAdmin = this.orgAdmin.filter(orgRoleRelation => (orgRoleRelation.Id !== value.rightId));
                    });
            }
        }

        public delete(module, rightId) {
            var id = rightId.Id;
            this.$http.delete(`/odata/${module}(${id})`)
                .then(() => {
                    if (module === "OrganizationUnitRights") {
                        this.orgRoles.del(id);
                        this.vmOrgUnits = this.vmOrgUnits.filter(bar => (bar.Id !== id));

                    }
                    if (module === "ItProjectRights") {
                        this.projectRoles.del(id);
                        this.vmProject = this.vmProject.filter(bar => (bar.Id !== id));

                    }
                    if (module === "ItSystemRights") {
                        this.systemRoles.del(id);
                        this.vmSystem = this.vmSystem.filter(bar => (bar.Id !== id));
                    }
                    if (module === "ItContractRights") {
                        this.contractRoles.del(id);
                        this.vmItContracts = this.vmItContracts.filter(bar => (bar.Id !== id));
                    }
                    if (module === "OrganizationRights") {
                        this.adminRoles.del(id);
                        this.vmOrgAdmin = this.vmOrgAdmin.filter(bar => (bar.Id !== id));
                    }
                });
        }

        public ok() {
            this.patchData();
            this.$uibModalInstance.close();
            this.notify.addSuccessMessage("Brugerens roller er ændret");
        }

        public assign() {
            this.patchData();
            this.notify.addSuccessMessage("Brugerens roller er ændret");
            this.disableBtns(true);
        }

        public cancel() {
            this.$uibModalInstance.dismiss();
        }

        public deleteUser() {
            this.deleteAllUserRoles(this.vmOrgUnits, "OrganizationUnitRights");
            this.deleteAllUserRoles(this.vmProject, "ItProjectRights");
            this.deleteAllUserRoles(this.vmSystem, "ItSystemRights");
            this.deleteAllUserRoles(this.vmItContracts, "ItContractRights");
            this.deleteAllUserRoles(this.orgAdmin, "OrganizationRights");
            this.$uibModalInstance.close();
        }

        public deleteSelectedRoles() {
            if (!confirm('Er du sikker på du vil slette de valgte roller?')) {
                return;
            }

            var orgRoles = this.orgRoles;
            var projRoles = this.projectRoles;
            var sysRoles = this.systemRoles;
            var contRoles = this.contractRoles;
            var adminRoles = this.adminRoles;

            if (orgRoles != null) {
                _.each(orgRoles.items,
                    (value, key) => {
                        this.$http.delete(`/odata/OrganizationUnitRights(${value.rightId})`)
                            .then(() => this.orgRoles.del(value.rightId));
                        this.vmOrgUnits = this.vmOrgUnits.filter(bar => (bar.Id !== value.rightId));
                    });
            }

            if (projRoles != null) {
                _.each(projRoles.items,
                    (value, key) => {
                        this.$http.delete(`/odata/ItProjectRights(${value.rightId})`)
                            .then(() => this.projectRoles.del(value.rightId));
                        this.vmProject = this.vmProject.filter(bar => (bar.Id !== value.rightId));
                    });
            }

            if (sysRoles != null) {
                _.each(sysRoles.items,
                    (value, key) => {
                        this.$http.delete(`/odata/ItSystemRights(${value.rightId})`)
                            .then(() => this.systemRoles.del(value.rightId));
                        this.vmSystem = this.vmSystem.filter(bar => (bar.Id !== value.rightId));
                    });
            }

            if (contRoles != null) {
                _.each(contRoles.items,
                    (value, key) => {
                        this.$http.delete(`/odata/ItContractRights(${value.rightId})`)
                            .then(() => this.contractRoles.del(value.rightId));
                        this.vmItContracts = this.vmItContracts.filter(bar => (bar.Id !== value.rightId));
                    });
            }
            if (adminRoles != null) {
                _.each(adminRoles.items,
                    (value, key) => {
                        this.$http.delete(`/odata/OrganizationRights(${value.rightId})`)
                            .then(() => this.adminRoles.del(value.rightId));
                        this.vmOrgAdmin = this.vmOrgAdmin.filter(bar => (bar.Id !== value.rightId));
                    });
            }

            this.notify.addSuccessMessage("Rollerne er slettede.");
            this.disableBtns(true);
        }

        public deleteAllUserRoles(roles: any, module: string) {
            for (var obj of roles) {
                var id = obj.Id;
                this.$http.delete(`/odata/${module}(${id})`)
                    .then(this.notify.addSuccessMessage("Brugeren og dennes roller er slettet fra organisationen"));
            }
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
                                        .then((currentUser) => $http.get(`/odata/Users(${$stateParams["id"]})?$expand=OrganizationRights($filter=OrganizationId eq ${currentUser.currentOrganizationId})`)
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
                                projects: [
                                    "$http", "ItProjectService", "userService",
                                    ($http: ng.IHttpService, itProjects, userService) =>
                                        userService.getUser()
                                            .then((currentUser) => itProjects.GetProjectDataById($stateParams["id"], `${currentUser.currentOrganizationId}`)
                                                .then(projectResult => projectResult.data.value))
                                ],
                                //Henter data for de forskellige collections ved brug er servicen "ItSystemService"
                                system: [
                                    "$http", "ItSystemService", "userService",
                                    ($http: ng.IHttpService, itSystems, userService) =>
                                    userService.getUser()
                                        .then((currentUser) => itSystems.GetSystemDataByIdFiltered($stateParams["id"], `${currentUser.currentOrganizationId}`)
                                            .then(systemResult => systemResult.data.value))
                                ],
                                //Henter data for de forskellige collections ved brug er servicen "ItContractService"
                                itContracts: [
                                    "$http", "ItContractsService", "userService",
                                    ($http: ng.IHttpService, itContracts, userService) =>
                                        userService.getUser()
                                            .then((currentUser) => itContracts.GetContractDataById($stateParams["id"], `${currentUser.currentOrganizationId}`)
                                                .then(systemResult => systemResult.data.value))
                                ],
                                //Henter data for de forskellige collections ved brug er servicen "OrganizationService"
                                orgUnits: [
                                    "$http", "organizationService", "userService",
                                    ($http: ng.IHttpService, organizationService, userService) =>
                                        userService.getUser()
                                            .then((currentUser) => organizationService.GetOrganizationUnitDataById($stateParams["id"], `${currentUser.currentOrganizationId}`)
                                                .then(result => result.data.value))
                                ],
                                orgAdmin: ["$http", "userService", "organizationService",
                                    ($http: ng.IHttpService, userService, organizationService) =>
                                        userService.getUser()
                                            .then((currentUser) => organizationService.GetOrganizationData($stateParams["id"], `${currentUser.currentOrganizationId}`)
                                                .then(result => result.data.value))
                                ],
                                text: ["$http", "$sce",
                                    ($http: ng.IHttpService, $sce) => {
                                        return $http.get("odata/HelpTexts?$filter=Key eq 'user_deletion_modal_text'")
                                            .then((result: any) => {
                                                if (result.data.value.length) {
                                                    return $sce.trustAsHtml(result.data.value[0].Description);
                                                } else {
                                                    return "Ingen hjælpetekst defineret.";
                                                }
                                        });
                                    }
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
                                $state.go("^", null, { reload: true });
                            });
                    }
                ]
            });
        }]);
}
