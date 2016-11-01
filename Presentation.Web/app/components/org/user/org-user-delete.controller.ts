module Kitos.Organization.Users {
    "use strict";

    //Controller til at vise en brugers roller i en organisation
    class DeleteOrganizationUserController {

        public vmProject: any;
        public vmProjectRights: any;
        public vmProjectRoles: any;
        public vmSystemRoles: any;
        public vmSystemRights: any;
        public vmSystem: any;
        public vmItContractsRoles: any;
        public vmItContractsRights: any;
        public vmItContracts: any;
        public vmOrganisationRights: any;
        public vmGetUsers: any;

        private userId: number;
        private firstName: string;
        private lastName: string;
        private email: string;

        // injecter resolve request i ctoren
        public static $inject: string[] = [
            "$uibModalInstance",
            "$http",
            "$q",
            "notify",
            "user",
            "currentUser",
            "projectRoles",
            "projectRights",
            "projects",
            "systemRoles",
            "systemRights",
            "system",
            "itContractsRoles",
            "itContractsRights",
            "itContracts",
            "organisationRights",
            "getUsers",
            "_"];

        constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private $http: IHttpServiceWithCustomConfig,
            private $q: ng.IQService,
            private notify,
            private user: Models.IUser,
            private currentUser: Services.IUser,
            public projectRoles,
            public projectRights,
            public projects,
            public systemRoles,
            public systemRights,
            public system,
            public itContractsRoles,
            public itContractsRights,
            public itContracts,
            public organisationRights,
            public getUsers,
            private _: ILoDashWithMixins) {

            this.userId = user.Id;
            this.firstName = user.Name;
            this.lastName = user.LastName;
            this.email = user.Email;

            this.vmProjectRights = projectRights;
            this.vmProjectRoles = projectRoles;
            this.vmProject = projects;
            this.vmSystemRights = systemRights;
            this.vmSystemRoles = systemRoles;
            this.vmSystem = system;
            this.vmItContractsRights = itContractsRights;
            this.vmItContractsRoles = itContractsRoles;
            this.vmItContracts = itContracts;
            this.vmOrganisationRights = organisationRights;
            this.vmGetUsers = getUsers;

            console.log("test");
            console.log(currentUser.currentOrganizationId);
            console.log(organisationRights);
            console.log(getUsers);
        }

        public ok() {
            this.$uibModalInstance.close();
        }

        public assign() {
            this.$uibModalInstance.dismiss();
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
                                //Henter data for de forskellige collections ved brug er servicen "ItProjectService"
                                projectRoles: ["ItProjectService", (itProjectRoles) =>
                                    itProjectRoles.GetAllProjectRoles()
                                        .then(roleResult => roleResult.data)
                                ],

                                projectRights: ["ItProjectService", (itProjectRights) =>
                                    itProjectRights.GetProjectRightsById($stateParams["id"])
                                        .then(rightsResult => rightsResult.data)
                                ],

                                projects: ["ItProjectService", (itProjects) =>
                                    itProjects.GetAllProjects()
                                        .then(projectResult => projectResult.data)
                                ],

                                systemRoles: ["ItSystemService", (itSystemRoles) =>
                                    itSystemRoles.GetAllSystemRoles()
                                        .then(roleResult => roleResult.data)
                                ],

                                systemRights: ["ItSystemService", (itSystemRights) =>
                                    itSystemRights.GetSystemRightsById($stateParams["id"])
                                        .then(rightsResult => rightsResult.data)
                                ],

                                system: ["ItSystemService", (itSystems) =>
                                    itSystems.GetAllSystems()
                                        .then(systemResult => systemResult.data)
                                ],

                                itContractsRoles: ["ItContractsService", (itContractsRoles) =>
                                    itContractsRoles.GetAllItContractRoles()
                                        .then(systemResult => systemResult.data)
                                ],

                                itContractsRights: ["ItContractsService", (itContractsRights) =>
                                    itContractsRights.GetItContractRightsById($stateParams["id"])
                                        .then(systemResult => systemResult.data)
                                ],

                                itContracts: ["ItContractsService", (itContracts) =>
                                    itContracts.GetAllItContracts()
                                        .then(systemResult => systemResult.data)
                                ],

                                organisationRights: ["$http", "userService",
                                    ($http: ng.IHttpService, userService) =>
                                        userService.getUser()
                                            .then((currentUser) => $http
                                                .get(`odata/Organizations(${currentUser.currentOrganizationId})/Rights?$filter=Role eq '0'`)
                                                .then(result => result.data))
                                ],
                                getUsers: ["UserGetService", (userGet) =>
                                    userGet.GetAllUsers()
                                        .then(users => users.data)
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
