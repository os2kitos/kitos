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

        public vmProject: any;
        public vmSystem: any;
        public vmItContracts: any;
        public vmGetUsers: any;
        public vmOrgUnits: any;
        public vmHasAdminRoles: boolean;
        public vmUsersInOrganization: any;
        public vmFullName: any;
        public selecterUserId: any;

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
            "usersInOrganization",
            "projects",
            "system",
            "itContracts",
            "orgUnits",
            "_"];

        constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private $http: IHttpServiceWithCustomConfig,
            private $q: ng.IQService,
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
        }

        public checkBoxTrueValue = (item) => {
            var data = { selectedItem: item }
            //console.log(item);
        }

        public setSelectedUser = (item) => {
            //var data = { selectedItem: item }
            //console.log(data.selectedItem);
            //var fullName = item.Name;
            //console.log(fullName);
            //this.vmFullName = 
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
