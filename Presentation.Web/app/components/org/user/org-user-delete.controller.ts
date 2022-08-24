module Kitos.Organization.Users {
    "use strict";
    
    interface IAssignedRightViewModel extends Models.ViewModel.Organization.IHasSelection {
        right: Models.Users.IAssignedRightDTO;
    }

    interface IAssignedAdminRoleViewModel extends Models.ViewModel.Organization.IHasSelection {
        role: Models.OrganizationRole;
    }

    interface IRootAssignedRightsWithGroupSelectionViewModel extends Models.ViewModel.Organization.IHasSelection {
        rights: IAssignedRightViewModel[];
    }

    interface IRootAssignedAdminRolesWithGroupSelectionViewModel extends Models.ViewModel.Organization.IHasSelection {
        rights: IAssignedAdminRoleViewModel[];
    }

    type UpdateSourceCollection = (collection: Array<IAssignedRightViewModel>) => void;
    type RoleSelectionSnapshot = { sourceCollection: Array<IAssignedRightViewModel>, updateSourceCollection: UpdateSourceCollection, selectedModels: Array<IAssignedRightViewModel> };

    //Controller til at vise en brugers roller i en organisation
    class DeleteOrganizationUserController {
        vmOrgRoot: IRootAssignedRightsWithGroupSelectionViewModel;
        vmContractRoot: IRootAssignedRightsWithGroupSelectionViewModel;
        vmDprRoot: IRootAssignedRightsWithGroupSelectionViewModel;
        vmProjectRoot: IRootAssignedRightsWithGroupSelectionViewModel;
        vmSystemRoot: IRootAssignedRightsWithGroupSelectionViewModel;
        vmAdminRoot: IRootAssignedAdminRolesWithGroupSelectionViewModel;

        areAllOrgRightsSelected: boolean;
        areAllProjectRightsSelected: boolean;
        areAllAdminRightsSelected: boolean;
        areAllSystemRightsSelected: boolean;
        areAllContractRightsSelected: boolean;
        areAllDprRightsSelected: boolean;

        vmGetUsers: any;
        vmUsersInOrganization: Array<Models.IUser>;
        selectedUser: Models.IUser | undefined | null;
        curOrganization: string;
        dirty: boolean;
        disabled: boolean;
        noRights: boolean;
        anySelections = false;
        allSelections = false;
        roleViewModelCallbacks: Users.IOrgUserRolesTableCallbacks<IAssignedRightViewModel>;

        readonly firstName: string;
        readonly lastName: string;
        readonly email: string;
        readonly vmText: string;

        static $inject: string[] = [
            "$uibModalInstance",
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
            private readonly userToModify: Models.IUser,
            private readonly loggedInUser: Services.IUser,
            usersInOrganization: Array<Models.IUser>,
            private readonly _: ILoDashWithMixins,
            text,
            allRoles: Models.Users.UserRoleAssigmentDTO,
            private readonly userRoleAdministrationService: Services.IUserRoleAdministrationService) {

            this.firstName = userToModify.Name;
            this.lastName = userToModify.LastName;
            this.email = userToModify.Email;

            const vmAdminRights = allRoles.administrativeAccessRoles.map(role => {
                return {
                    selected: false,
                    role: role
                }
            });
            const vmOrgRights = this.createViewModel(allRoles.rights, Models.Users.BusinessRoleScope.OrganizationUnit);
            const vmContractRights = this.createViewModel(allRoles.rights, Models.Users.BusinessRoleScope.ItContract);
            const vmDprRights = this.createViewModel(allRoles.rights, Models.Users.BusinessRoleScope.DataProcessingRegistration);
            const vmProjectRights = this.createViewModel(allRoles.rights, Models.Users.BusinessRoleScope.ItProject);
            const vmSystemRights = this.createViewModel(allRoles.rights, Models.Users.BusinessRoleScope.ItSystemUsage);

            this.vmOrgRoot = {
                rights: vmOrgRights,
                selected: false
            }
            this.vmContractRoot = {
                rights: vmContractRights,
                selected: false
            }
            this.vmDprRoot = {
                rights: vmDprRights,
                selected: false
            }
            this.vmProjectRoot = {
                rights: vmProjectRights,
                selected: false
            }
            this.vmSystemRoot = {
                rights: vmSystemRights,
                selected: false
            }
            this.vmAdminRoot = {
                rights: vmAdminRights,
                selected: false
            }
            
            this.vmUsersInOrganization = usersInOrganization.filter(x => x.Id !== userToModify.Id);

            this.curOrganization = loggedInUser.currentOrganizationName;
            this.vmText = text;
            this.updateNoRights();
            this.roleViewModelCallbacks = {
                delete: right => this.deleteRight(right),
                selectionChanged: () => this.updateAnySelections(),
                selectOrDeselectGroup: (rights: Models.ViewModel.Organization.IHasSelection[]) => this.selectOrDeselectGroup(rights)
            };
        }

        private updateNoRights() {
            this.noRights = this.getAllRoles().length === 0;
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

        updateAnySelections() {
            let anySelectionsFound = false;
            let allSelectionsFound = false;

            const numberOfSelectedAdminRoles = this.collectSelectedAdminRoles().length;
            const selectedRoles = this.collectSelectedRoles();

            const numberOfSelectedRoles = selectedRoles.reduce((acc, role) => acc + role.selectedModels.length, 0);

            const totalRoles = this.getAllRoles().length;
            const totalSelectedRoles = numberOfSelectedAdminRoles + numberOfSelectedRoles;

            if (totalSelectedRoles > 0) {
                anySelectionsFound = true;
            }
            if (totalSelectedRoles === totalRoles) {
                allSelectionsFound = true;
            }

            this.updateGroupSelections();
            this.anySelections = anySelectionsFound;
            this.allSelections = allSelectionsFound;
        }

        deleteRight(viewModel: IAssignedRightViewModel) {
            let snapshot: RoleSelectionSnapshot | null = null;
            switch (viewModel.right.scope) {
                case Models.Users.BusinessRoleScope.ItProject:
                    snapshot = {
                        selectedModels: this.vmProjectRoot.rights.filter(r => r.right.rightId === viewModel.right.rightId),
                        sourceCollection: this.vmProjectRoot.rights,
                        updateSourceCollection: (newValues) => this.vmProjectRoot.rights = newValues
                    };
                    break;
                case Models.Users.BusinessRoleScope.ItContract:
                    snapshot = {
                        selectedModels: this.vmContractRoot.rights.filter(r => r.right.rightId === viewModel.right.rightId),
                        sourceCollection: this.vmContractRoot.rights,
                        updateSourceCollection: (newValues) => this.vmContractRoot.rights = newValues
                    };
                    break;
                case Models.Users.BusinessRoleScope.DataProcessingRegistration:
                    snapshot = {
                        selectedModels: this.vmDprRoot.rights.filter(r => r.right.rightId === viewModel.right.rightId),
                        sourceCollection: this.vmDprRoot.rights,
                        updateSourceCollection: (newValues) => this.vmDprRoot.rights = newValues
                    };
                    break;
                case Models.Users.BusinessRoleScope.ItSystemUsage:
                    snapshot = {
                        selectedModels: this.vmSystemRoot.rights.filter(r => r.right.rightId === viewModel.right.rightId),
                        sourceCollection: this.vmSystemRoot.rights,
                        updateSourceCollection: (newValues) => this.vmSystemRoot.rights = newValues
                    };
                    break;
                case Models.Users.BusinessRoleScope.OrganizationUnit:
                    snapshot = {
                        selectedModels: this.vmOrgRoot.rights.filter(r => r.right.rightId === viewModel.right.rightId),
                        sourceCollection: this.vmOrgRoot.rights,
                        updateSourceCollection: (newValues) => this.vmOrgRoot.rights = newValues
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
            const selectedAdminRoles = this.vmAdminRoot.rights.filter(x => x.role === role);
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

            const selectedRoles = this.collectSelectedRoles();
            const selectedAdminRoles = this.collectSelectedAdminRoles();
            this.userRoleAdministrationService
                .transferAssignedRoles(
                    this.loggedInUser.currentOrganizationId,
                    this.userToModify.Id,
                    this.selectedUser.Id,
                    {
                        administrativeAccessRoles: selectedAdminRoles.map(x => x.role),
                        rights: selectedRoles.reduce<Array<Models.Users.IAssignedRightDTO>>((result, next) => result.concat(next.selectedModels.map(x => x.right)),
                            [])
                    }
                ).then(success => {
                        if (success) {
                            this.updateViewModels(selectedRoles, selectedAdminRoles);
                        }
                    }
                );
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

        updateViewModels(removedRoles: Array<RoleSelectionSnapshot>, removedAdminRoles: Array<IAssignedAdminRoleViewModel>) {
            this.vmAdminRoot.rights = this.vmAdminRoot.rights.filter(vm => removedAdminRoles.indexOf(vm) === -1);
            for (var roles of removedRoles) {
                roles.updateSourceCollection(
                    roles.sourceCollection.filter(vm => roles.selectedModels.indexOf(vm) === -1));
            }
            this.updateNoRights();
            this.updateAnySelections();
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
                        this.updateViewModels(selectedRoles, selectedAdminRoles);
                    }
                }
                );
        }

        selectOrDeselectGroup(rights: Models.ViewModel.Organization.IHasSelection[]) {
            const areAllSelected = rights.filter(vm => !vm.selected).length < 1;
            const targetSelectValue = !areAllSelected;
            this.setSelectGroupToValue(rights, targetSelectValue);
            this.updateAnySelections();
        }

        selectAll() {
            this.changeAllSelections(true);
            this.updateAnySelections();
        }

        deselectAll() {
            this.changeAllSelections(false);
            this.updateAnySelections();
        }

        selectOrDeselectAll() {
        }

        private getAllRoles() {
            let roles = [];

            roles = roles.concat(this.vmAdminRoot.rights);
            roles = roles.concat(this.vmDprRoot.rights);
            roles = roles.concat(this.vmOrgRoot.rights);
            roles = roles.concat(this.vmProjectRoot.rights);
            roles = roles.concat(this.vmSystemRoot.rights);
            roles = roles.concat(this.vmContractRoot.rights);
            
            return roles;
        }

        private setSelectGroupToValue(rights: Models.ViewModel.Organization.IHasSelection[], targetValue: boolean) {
            rights.forEach(vm => {
                vm.selected = targetValue;
            });
        }

        private changeAllSelections(targetValue: boolean) {
            this.changeGroupSelectionStatus(this.vmOrgRoot, targetValue);
            this.changeGroupSelectionStatus(this.vmContractRoot, targetValue);
            this.changeGroupSelectionStatus(this.vmDprRoot, targetValue);
            this.changeGroupSelectionStatus(this.vmProjectRoot, targetValue);
            this.changeGroupSelectionStatus(this.vmSystemRoot, targetValue);
            this.changeGroupSelectionStatus(this.vmAdminRoot, targetValue);
        }

        private changeGroupSelectionStatus(groupRoot: IRootAssignedRightsWithGroupSelectionViewModel | IRootAssignedAdminRolesWithGroupSelectionViewModel, targetValue: boolean) {
            this.setSelectGroupToValue(groupRoot.rights, targetValue);
            groupRoot.selected = targetValue;
        }

        private collectSelectedRolesFromSource(sourceCollection: Array<IAssignedRightViewModel>, updateSourceCollection: UpdateSourceCollection): RoleSelectionSnapshot {
            const selected = sourceCollection.filter(x => x.selected);
            return {
                sourceCollection: sourceCollection,
                updateSourceCollection: updateSourceCollection,
                selectedModels: selected
            };
        }

        private updateGroupSelections() {

            this.checkAndChangeGroupSelectionStatus(this.vmContractRoot);
            this.checkAndChangeGroupSelectionStatus(this.vmProjectRoot);
            this.checkAndChangeGroupSelectionStatus(this.vmSystemRoot);
            this.checkAndChangeGroupSelectionStatus(this.vmDprRoot);
            this.checkAndChangeGroupSelectionStatus(this.vmOrgRoot);

            const selectedAdminRights = this.collectSelectedAdminRoles();
            if (selectedAdminRights.length < this.vmAdminRoot.rights.length) {
                this.vmAdminRoot.selected = false;
                return;
            } 

            this.vmAdminRoot.selected = true;
        }

        private checkAndChangeGroupSelectionStatus(groupRoot: IRootAssignedRightsWithGroupSelectionViewModel) {
            const selectedContractRights =
                this.collectSelectedRolesFromSource(groupRoot.rights, (newRights) => this.vmContractRoot.rights = newRights);
            
            if (selectedContractRights.selectedModels.length < groupRoot.rights.length) {
                groupRoot.selected = false;
                return;
            }
            
            groupRoot.selected = true;
        }

        private collectSelectedRoles(): Array<RoleSelectionSnapshot> {

            const result = new Array<RoleSelectionSnapshot>();
            
            result.push(this.collectSelectedRolesFromSource(this.vmContractRoot.rights, (newRights) => this.vmContractRoot.rights = newRights));
            result.push(this.collectSelectedRolesFromSource(this.vmProjectRoot.rights, (newRights) => this.vmProjectRoot.rights = newRights));
            result.push(this.collectSelectedRolesFromSource(this.vmSystemRoot.rights, (newRights) => this.vmSystemRoot.rights = newRights));
            result.push(this.collectSelectedRolesFromSource(this.vmDprRoot.rights, (newRights) => this.vmDprRoot.rights = newRights));
            result.push(this.collectSelectedRolesFromSource(this.vmOrgRoot.rights, (newRights) => this.vmOrgRoot.rights = newRights));

            return result;
        }

        private collectSelectedAdminRoles(): Array<IAssignedAdminRoleViewModel> {
            const selectedAdminRoles = this.vmAdminRoot.rights.filter(r => r.selected);
            
            return selectedAdminRoles;
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
