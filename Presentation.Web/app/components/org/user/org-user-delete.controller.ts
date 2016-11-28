module Kitos.Organization.Users {
    import OrganizationService = Kitos.Services.OrganizationService;
    "use strict";

    class UserRole {
        public modul: any;
        public rightId: any;
        public user: any;
        public userId: number;
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
        public selecterUser: any;
        public isUserSelected: boolean;

        private userId: number;
        private firstName: string;
        private lastName: string;
        private email: string;
        private itemSelected: boolean;

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
            "_"
            ];

        constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private $http: IHttpServiceWithCustomConfig,
            private $q: ng.IQService,
            private $state: ng.ui.IStateService,
            private $scope,
            private notify,
            private user: any,
            private currentUser: Services.IUser,
            private usersInOrganization: Models.IUser,
            public projects,
            public system,
            public itContracts,
            public orgUnits,
            public orgAdmin,
            private _: ILoDashWithMixins) {

            this.userId = user.Id;
            this.firstName = user.Name;
            this.lastName = user.LastName;
            this.email = user.Email;

            this.initCollections(orgUnits, this.vmOrgRoles);
            this.initCollections(projects, this.vmProjectRoles);
            this.initCollections(system, this.vmSystemRoles);
            this.initCollections(itContracts, this.vmContractRoles);
            this.initCollections(orgAdmin, this.vmAdminRoles);

            this.vmUsersInOrganization = usersInOrganization;
            this.vmProject = projects;
            this.vmSystem = system;
            this.vmItContracts = itContracts;
            this.vmOrgUnits = orgUnits;
            this.vmOrgAdmin = orgAdmin.filter(bar => (bar.Role !== 'User'));
            this.itemSelected = false;
            this.isUserSelected = true;
        }

        public initCollections = (collection, output) => {
            for (var item of collection) {
                output.add(item.Id, item);
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
                        userId: data.Id,
                        objectId: object.ObjectId
                    }
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
        }

        public setSelectedUser = (item) => {
            if (item == null) {
                this.isUserSelected = true;
            } else {
                var data = JSON.parse(item);
                this.selecterUser = data;
                this.isUserSelected = false;
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
                            UserId: value.userId
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
                            UserId: value.userId
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
                            UserId: value.userId
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
                            UserId: value.userId
                        }
                        this.$http.patch(`/odata/ItContractRights(${value.rightId})`, payload)
                            .then(() => this.contractRoles.del(value.rightId));
                        this.vmItContracts = this.vmItContracts.filter(bar => (bar.Id !== value.rightId));
                    });
            }
            if (adminRoles != null) {
                _.each(adminRoles.items,
                    (value, key) => {
                        var id = value.rightId;
                        let payload = {
                            UserId: value.userId
                        }
                        this.$http.patch(`/odata/OrganizationRights(${value.rightId})`, payload)
                            .then(() => this.adminRoles.del(value.rightId));
                        console.log(id);
                        //this.vmOrgAdmin = this.vmOrgAdmin.filter(bar => (bar.Id !== value.rightId));
                        this.vmOrgAdmin = this.vmOrgAdmin.filter(bar => (bar.Id !== id));
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
                        console.log(this.vmOrgAdmin);
                        this.vmOrgAdmin = this.vmOrgAdmin.filter(bar => (bar.Id !== id));
                        console.log(this.vmOrgAdmin);
                    }
                });
        }

        public ok() {
            this.patchData();
            this.$uibModalInstance.close();
            this.notify.addSuccessMessage("Roller er ændret");
        }

        public assign() {
            this.patchData();
            this.notify.addSuccessMessage("Roller er ændret");
        }

        public cancel() {
            this.$uibModalInstance.dismiss();
        }

        public deleteUser() {
            this.$http.delete(`/odata/Users(${this.userId})`);
            this.$uibModalInstance.close();
            this.notify.addSuccessMessage("Brugeren og dennes roller er slettet");
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
                                ],
                                orgAdmin: ["$http", "userService", "organizationService", 
                                    ($http: ng.IHttpService, userService, organizationService) =>
                                        userService.getUser()
                                            .then((currentUser) => organizationService.GetOrganizationData($stateParams["id"], `${currentUser.currentOrganizationId}`)
                                            .then(result => result.data.value))
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
