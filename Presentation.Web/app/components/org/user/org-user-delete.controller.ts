module Kitos.Organization.Users {
    "use strict";

    interface IHasSelection {
        selected: boolean;
    }


    interface IAssignedRightViewModel extends IHasSelection {
        right: Models.Users.IAssignedRightDTO;
    }

    interface IAssignedAdminRoleViewModel extends IHasSelection {
        role: Models.OrganizationRole;
    }

    //Controller til at vise en brugers roller i en organisation
    class DeleteOrganizationUserController {
        vmOrgRights = new Array<IAssignedRightViewModel>();
        vmProjectRights = new Array<IAssignedRightViewModel>();
        vmAdminRights = new Array<IAssignedAdminRoleViewModel>();
        vmSystemRights = new Array<IAssignedRightViewModel>();
        vmContractRights = new Array<IAssignedRightViewModel>();
        vmDprRights = new Array<IAssignedRightViewModel>();

        vmGetUsers: any;
        vmUsersInOrganization: any;
        selectedUser: any;
        isUserSelected: boolean;
        curOrganization: string;
        dirty: boolean;
        disabled: boolean;
        noRights: boolean;

        readonly firstName: string;
        readonly lastName: string;
        readonly email: string;
        readonly vmText: string;

        static $inject: string[] = [
            "$uibModalInstance",
            "notify",
            "user",
            "usersInOrganization",
            "_",
            "text",
            "allRoles"
        ];

        constructor(
            private readonly $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private readonly notify,
            user: any,
            usersInOrganization: any,
            private readonly _: ILoDashWithMixins,
            text,
            allRoles: Models.Users.UserRoleAssigmentDTO) {

            this.firstName = user.Name;
            this.lastName = user.LastName;
            this.email = user.Email;

            this.vmAdminRights = allRoles.administrativeAccessRoles.map(role => {
                return {
                    selected: false,
                    role: role
                }
            });
            this.noRights = allRoles.administrativeAccessRoles.length === 0 && allRoles.rights.length === 0;
            this.vmOrgRights = this.createViewModel(allRoles.rights, Models.Users.BusinessRoleScope.OrganizationUnit);
            this.vmContractRights = this.createViewModel(allRoles.rights, Models.Users.BusinessRoleScope.ItContract);
            this.vmDprRights = this.createViewModel(allRoles.rights, Models.Users.BusinessRoleScope.DataProcessingRegistration);
            this.vmProjectRights = this.createViewModel(allRoles.rights, Models.Users.BusinessRoleScope.ItProject);
            this.vmSystemRights = this.createViewModel(allRoles.rights, Models.Users.BusinessRoleScope.ItSystemUsage);

            this.vmUsersInOrganization = usersInOrganization.filter(x => x.Id !== user.Id);

            this.isUserSelected = true;
            this.curOrganization = user.currentOrganizationName;
            this.disabled = true;
            this.vmText = text;
        }

        createViewModel = (fullCollection: Array<Models.Users.IAssignedRightDTO>, roleScope: Models.Users.BusinessRoleScope) => {
            return fullCollection
                .filter(entry => entry.scope === roleScope)
                .map(item => {
                    return {
                        right: item,
                        selected: false
                    };
                });
        }

        disableBtns(val) {
            this.disabled = val;
        }

        setSelectedUser = () => {
            if (this.selectedUser == null) {
                this.isUserSelected = true;
            } else {
                this.isUserSelected = false;
                this.disableBtns(this.isUserSelected);
            }
        }

        deleteRight(scope, rightId) {
            //TODO: also confirm here!
            //TODO: Invoke deleterights with some selected
        }

        deleteAdminRole(role: Models.OrganizationRole) {
            //TODO
        }

        assign() {
            //TODO: Should call a custom endpoint in stead and respect the promise
            //TODO: Run the scenario, and in the "then" update view models and disable btns
            //TODO: also confirm here!
            this.disableBtns(true);
        }

        cancel() {
            this.$uibModalInstance.dismiss();
        }

        deleteUser() {
            //TODO: Delete the user from org and then close the modal
            //TODO: also confirm here!
            this.$uibModalInstance.close();

        }

        deleteSelectedRoles() {
            if (!confirm('Er du sikker på du vil slette de valgte roller?')) {
                return;
            }

            //TODO: Delete the selected view models, then remove them from the state if successful
            this.disableBtns(true);
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
                                ],
                                allRoles: ["userRoleAdministrationService", "userService",
                                    (userRoleAdministrationService: Services.IUserRoleAdministrationService, userService: Services.IUserService) =>
                                        userService
                                            .getUser()
                                            .then(user => userRoleAdministrationService.getAssignedRoles(user.currentOrganizationId, $stateParams["id"]))
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
