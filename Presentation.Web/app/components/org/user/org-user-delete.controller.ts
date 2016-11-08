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

    //Controller til at vise en brugers roller i en organisation
    class DeleteOrganizationUserController {
        public vm: IDeleteViewModel;
        public isUserGlobalAdmin = false;
        public isUserLocalAdmin = false;
        public isUserOrgAdmin = false;
        public isUserProjectAdmin = false;
        public isUserSystemAdmin = false;
        public isUserContractAdmin = false;
        public isUserReportAdmin = false;

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
        public vmOrgUnitRights: any;
        public vmOrgUnitRoles: any;
        public vmOrgUnits: any;

        private userId: number;
        private firstName: string;
        private lastName: string;
        private email: string;
        private originalVm;

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
            "getUsers",
            "orgUnitRoles",
            "orgUnitRights",
            "orgUnits",
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
            public getUsers,
            public orgUnitRoles,
            public orgUnitRights,
            public orgUnits,
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
            this.vmGetUsers = getUsers;
            this.vmOrgUnitRights = orgUnitRights;
            this.vmOrgUnitRoles = orgUnitRoles;
            this.vmOrgUnits = orgUnits;

            var userVm: IDeleteViewModel = {
                isLocalAdmin: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.LocalAdmin }) !== undefined,
                isOrgAdmin: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.OrganizationModuleAdmin }) !== undefined,
                isProjectAdmin: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.ProjectModuleAdmin }) !== undefined,
                isSystemAdmin: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.SystemModuleAdmin }) !== undefined,
                isContractAdmin: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.ContractModuleAdmin }) !== undefined,
                isReportAdmin: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.ReportModuleAdmin }) !== undefined
            };
            this.vm = userVm;
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
                                    ($http, userService) =>
                                        userService.getUser()
                                            .then((currentUser) => $http
                                                .get(`/odata/Users(${$stateParams["id"]})?$expand=OrganizationRights($filter=OrganizationId eq ${currentUser.currentOrganizationId})`)
                                                .then(result => result.data.value))
                                ],
                                //Henter data for de forskellige collections ved brug er servicen "ItProjectService"
                                projectRoles: ["ItProjectService", (itProjectRoles) =>
                                    itProjectRoles.GetAllProjectRoles()
                                        .then(roleResult => roleResult.data.value)
                                ],

                                projectRights: ["ItProjectService", (itProjectRights) =>
                                    itProjectRights.GetProjectRightsById($stateParams["id"])
                                        .then(rightsResult => rightsResult.data.value)
                                ],

                                projects: ["ItProjectService", (itProjects) =>
                                    itProjects.GetAllProjects()
                                        .then(projectResult => projectResult.data.value)
                                ],

                                systemRoles: ["ItSystemService", (itSystemRoles) =>
                                    itSystemRoles.GetAllSystemRoles()
                                        .then(roleResult => roleResult.data.value)
                                ],

                                systemRights: ["ItSystemService", (itSystemRights) =>
                                    itSystemRights.GetSystemRightsById($stateParams["id"])
                                        .then(rightsResult => rightsResult.data.value)
                                ],

                                system: ["ItSystemService", (itSystems) =>
                                    itSystems.GetAllSystems()
                                        .then(systemResult => systemResult.data.value)
                                ],

                                itContractsRoles: ["ItContractsService", (itContractsRoles) =>
                                    itContractsRoles.GetAllItContractRoles()
                                        .then(systemResult => systemResult.data.value)
                                ],

                                itContractsRights: ["ItContractsService", (itContractsRights) =>
                                    itContractsRights.GetItContractRightsById($stateParams["id"])
                                        .then(systemResult => systemResult.data.value)
                                ],

                                itContracts: ["ItContractsService", (itContracts) =>
                                    itContracts.GetAllItContracts()
                                        .then(systemResult => systemResult.data.value)
                                ],
                                
                                getUsers: ["UserGetService", (userGet) =>
                                    userGet.GetAllUsers()
                                        .then(users => users.data.value)
                                ],

                                orgUnitRoles: ["organizationService", (orgUnitRoles) =>
                                    orgUnitRoles.GetAllOrganizationUnitRoles()
                                        .then(result => result.data.value)
                                ],

                                orgUnitRights: ["organizationService", (orgUnitRights) =>
                                    orgUnitRights.GetOrganisationRightsById($stateParams["id"])
                                        .then(result => result.data.value)
                                ],

                                orgUnits: ["organizationService", (orgUnits) =>
                                    orgUnits.GetOrganizationUnitById()
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
