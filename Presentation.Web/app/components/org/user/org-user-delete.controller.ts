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

    type UpdateSourceCollection = (collection: Array<IAssignedRightViewModel>) => void;
    type RoleSelectionSnapshot = { sourceCollection: Array<IAssignedRightViewModel>, updateSourceCollection: UpdateSourceCollection, selectedModels: Array<IAssignedRightViewModel> };

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
        anySelections = false;

        readonly firstName: string;
        readonly lastName: string;
        readonly email: string;
        readonly vmText: string;

        static $inject: string[] = [
            "$uibModalInstance",
            "notify",
            "userToModify",
            "loggedInUser",
            "usersInOrganization",
            "_",
            "text",
            "allRoles",
            "userRoleAdministrationService"
        ];

        constructor(
            private readonly $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private readonly notify,
            private readonly userToModify: Models.IUser,
            private readonly loggedInUser: Services.IUser,
            usersInOrganization: any,
            private readonly _: ILoDashWithMixins,
            text,
            allRoles: Models.Users.UserRoleAssigmentDTO,
            private readonly userRoleAdministrationService: Services.IUserRoleAdministrationService) {

            this.firstName = userToModify.Name;
            this.lastName = userToModify.LastName;
            this.email = userToModify.Email;

            this.vmAdminRights = allRoles.administrativeAccessRoles.map(role => {
                return {
                    selected: false,
                    role: role
                }
            });

            this.vmOrgRights = this.createViewModel(allRoles.rights, Models.Users.BusinessRoleScope.OrganizationUnit);
            this.vmContractRights = this.createViewModel(allRoles.rights, Models.Users.BusinessRoleScope.ItContract);
            this.vmDprRights = this.createViewModel(allRoles.rights, Models.Users.BusinessRoleScope.DataProcessingRegistration);
            this.vmProjectRights = this.createViewModel(allRoles.rights, Models.Users.BusinessRoleScope.ItProject);
            this.vmSystemRights = this.createViewModel(allRoles.rights, Models.Users.BusinessRoleScope.ItSystemUsage);

            this.vmUsersInOrganization = usersInOrganization.filter(x => x.Id !== userToModify.Id);

            this.isUserSelected = false;
            this.curOrganization = loggedInUser.currentOrganizationName;
            this.vmText = text;
            this.updateNoRights();
        }

        private updateNoRights() {
            this.noRights =
                this.vmOrgRights.length === 0 &&
                this.vmContractRights.length === 0 &&
                this.vmDprRights.length === 0 &&
                this.vmProjectRights.length === 0 &&
                this.vmSystemRights.length === 0 &&
                this.vmAdminRights.length === 0;
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

        setSelectedUser = () => {
            if (this.selectedUser == null) {
                this.isUserSelected = true;
            } else {
                this.isUserSelected = false;
            }
        }

        updateAnySelections() {
            let anySelectionsFound = false;
            if (this.collectSelectedAdminRoles().length > 0) {
                anySelectionsFound = true;
            }
            else if (this.collectSelectedRoles().filter(x => x.selectedModels.length > 0).length > 0) {
                anySelectionsFound = true;
            }
            this.anySelections = anySelectionsFound;
        }

        deleteRight(viewModel: IAssignedRightViewModel) {
            let snapshot: RoleSelectionSnapshot | null = null;
            switch (viewModel.right.scope) {
                case Models.Users.BusinessRoleScope.ItProject:
                    snapshot = {
                        selectedModels: this.vmProjectRights.filter(r => r.right.rightId === viewModel.right.rightId),
                        sourceCollection: this.vmProjectRights,
                        updateSourceCollection: (newValues) => this.vmProjectRights = newValues
                    };
                    break;
                case Models.Users.BusinessRoleScope.ItContract:
                    snapshot = {
                        selectedModels: this.vmContractRights.filter(r => r.right.rightId === viewModel.right.rightId),
                        sourceCollection: this.vmContractRights,
                        updateSourceCollection: (newValues) => this.vmContractRights = newValues
                    };
                    break;
                case Models.Users.BusinessRoleScope.DataProcessingRegistration:
                    snapshot = {
                        selectedModels: this.vmDprRights.filter(r => r.right.rightId === viewModel.right.rightId),
                        sourceCollection: this.vmDprRights,
                        updateSourceCollection: (newValues) => this.vmDprRights = newValues
                    };
                    break;
                case Models.Users.BusinessRoleScope.ItSystemUsage:
                    snapshot = {
                        selectedModels: this.vmSystemRights.filter(r => r.right.rightId === viewModel.right.rightId),
                        sourceCollection: this.vmSystemRights,
                        updateSourceCollection: (newValues) => this.vmSystemRights = newValues
                    };
                    break;
                case Models.Users.BusinessRoleScope.OrganizationUnit:
                    snapshot = {
                        selectedModels: this.vmOrgRights.filter(r => r.right.rightId === viewModel.right.rightId),
                        sourceCollection: this.vmOrgRights,
                        updateSourceCollection: (newValues) => this.vmOrgRights = newValues
                    };
                    break;
            }

            if (snapshot !== null) {
                if (!confirm('Er du sikker på, at du vil slette rollen?')) {
                    return;
                }
                this.deleteRoles([snapshot], []);
            }
        }

        deleteAdminRole(role: Models.OrganizationRole) {
            const selectedAdminRoles = this.vmAdminRights.filter(x => x.role === role);
            if (selectedAdminRoles.length != 0) {
                if (!confirm('Er du sikker på, at du vil slette rollen?')) {
                    return;
                }
                this.deleteRoles([], selectedAdminRoles);
            }
        }

        assign() {
            if (!confirm('Er du sikker på, at du vil overføre rollerne?')) {
                return;
            }
            //TODO: Should call a custom endpoint in stead and respect the promise
            //TODO: Run the scenario, and in the "then" update view models and disable btns
            //TODO: also confirm here!
            const selectedRoles = this.collectSelectedRoles();
            const selectedAdminRoles = this.collectSelectedAdminRoles();
        }

        cancel() {
            this.$uibModalInstance.dismiss();
        }

        deleteUser() {
            if (!confirm('Er du sikker på, at du vil slette brugeren?')) {
                return;
            }
            this.userRoleAdministrationService.removeUser(this.loggedInUser.currentOrganizationId, this.userToModify.Id)
                .then(success => {
                    if (success) {
                        this.$uibModalInstance.close();
                    }
                });
        }

        deleteSelectedRoles() {
            if (!confirm('Er du sikker på, at du vil slette de valgte roller?')) {
                return;
            }

            const selectedRoles = this.collectSelectedRoles();
            const selectedAdminRoles = this.collectSelectedAdminRoles();

            this.deleteRoles(selectedRoles, selectedAdminRoles);
        }

        deleteRoles(selectedRoles: Array<RoleSelectionSnapshot>, selectedAdminRoles: Array<IAssignedAdminRoleViewModel>) {
            this.userRoleAdministrationService
                .removeAssignedRoles(this.loggedInUser.currentOrganizationId,
                    this.userToModify.Id,
                    {
                        administrativeAccessRoles: selectedAdminRoles.map(x => x.role),
                        rights: selectedRoles.reduce<Array<Models.Users.IAssignedRightDTO>>(
                            (result, next) => result.concat(next.selectedModels.map(x => x.right)),
                            [])
                    }
                ).then(success => {
                    if (success) {
                        this.vmAdminRights = this.vmAdminRights.filter(vm => selectedAdminRoles.indexOf(vm) === -1);
                        for (var roles of selectedRoles) {
                            roles.updateSourceCollection(
                                roles.sourceCollection.filter(vm => roles.selectedModels.indexOf(vm) === -1));
                        }
                        this.updateNoRights();
                        this.updateAnySelections();
                    }
                }
                );
        }


        private collectSelectedRolesFromSource(sourceCollection: Array<IAssignedRightViewModel>, updateSourceCollection: UpdateSourceCollection): RoleSelectionSnapshot {
            const selected = sourceCollection.filter(x => x.selected);
            return {
                sourceCollection: sourceCollection,
                updateSourceCollection: updateSourceCollection,
                selectedModels: selected
            };
        }

        private collectSelectedRoles(): Array<RoleSelectionSnapshot> {

            const result = new Array<RoleSelectionSnapshot>();

            result.push(this.collectSelectedRolesFromSource(this.vmContractRights, (newRights) => this.vmContractRights = newRights));
            result.push(this.collectSelectedRolesFromSource(this.vmProjectRights, (newRights) => this.vmProjectRights = newRights));
            result.push(this.collectSelectedRolesFromSource(this.vmSystemRights, (newRights) => this.vmSystemRights = newRights));
            result.push(this.collectSelectedRolesFromSource(this.vmDprRights, (newRights) => this.vmDprRights = newRights));
            result.push(this.collectSelectedRolesFromSource(this.vmOrgRights, (newRights) => this.vmOrgRights = newRights));

            return result;
        }

        private collectSelectedAdminRoles(): Array<IAssignedAdminRoleViewModel> {

            return this.vmAdminRights.filter(r => r.selected);
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
                                loggedInUser: [
                                    "userService",
                                    (userService) => userService.getUser()
                                ],
                                userToModify: [
                                    "$http", "userService",
                                    ($http: ng.IHttpService, userService: Services.IUserService) =>
                                        userService.getUser().then(loggedInUser =>
                                            $http.get(`/odata/Users(${$stateParams["id"]})?$expand=OrganizationRights($filter=OrganizationId eq ${loggedInUser.currentOrganizationId})`)
                                                .then(result => result.data))
                                ],
                                usersInOrganization: [
                                    "$http", "userService", "UserGetService",
                                    ($http: ng.IHttpService, userService: Services.IUserService, userGetService) =>
                                        userService.getUser().then(loggedInUser =>
                                            userGetService.GetAllUsersFromOrganizationById(`${loggedInUser.currentOrganizationId}`)
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
                                        userService.getUser().then(loggedInUser =>
                                            userRoleAdministrationService.getAssignedRoles(loggedInUser.currentOrganizationId, $stateParams["id"]))
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
