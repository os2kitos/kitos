module Kitos.DataProcessing.Agreement.Edit.Roles {
    import IEditableAssignedRoleViewModel = Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel;
    "use strict";

    export class EditRolesDataProcessingAgreementController {
        static $inject: Array<string> = [
            "dataProcessingAgreementService",
            "notify",
            "hasWriteAccess",
            "dataProcessingAgreement",
            "dataProcessingAgreementRoles",
            "select2LoadingService",
        ];

        constructor(
            private readonly dataProcessingAgreementService: Services.DataProcessing.IDataProcessingAgreementService,
            private readonly notify,
            public hasWriteAccess,
            private readonly dataProcessingAgreement: Models.DataProcessing.IDataProcessingAgreementDTO,
            private readonly dataProcessingAgreementRoles: Models.DataProcessing.IDataProcessingRoleDTO[],
            private readonly select2LoadingService: Services.ISelect2LoadingService) {
        }

        headerName = this.dataProcessingAgreement.name;

        roles = _.map(this.dataProcessingAgreementRoles, role => this.mapToRoleOptions(role));

        selectedRoleIdAsString = this.roles[0].id.toString();
        selectedUser: Models.ViewModel.Generic.Select2OptionViewModel;

        userOptions = this.getAvailableUserOptions(() => parseInt(this.selectedRoleIdAsString));

        assignedRoles = _.orderBy(
            _.map(this.dataProcessingAgreement.assignedRoles, assignedRole => this.mapToEditableAssignedRole(assignedRole)),
            assignedRole => [assignedRole.role.name, assignedRole.user.name, assignedRole.user.email],
            "asc");

        lastSortedBy = "";
        shouldSortAsc = true;
        sortAssignedRoles(sortBy: string) {
            sortBy === this.lastSortedBy ? this.shouldSortAsc = !this.shouldSortAsc : this.shouldSortAsc = true;
            var sortDirection = this.shouldSortAsc ? "asc" : "desc";
            switch (sortBy) {
                case "roleName":
                    this.assignedRoles = _.orderBy(
                        this.assignedRoles,
                        assignedRole => [assignedRole.role.name, assignedRole.user.name, assignedRole.user.email],
                        sortDirection);
                    break;
                case "userName":
                    this.assignedRoles = _.orderBy(
                        this.assignedRoles,
                        assignedRole => [assignedRole.user.name, assignedRole.user.email, assignedRole.role.name],
                        sortDirection);
                    break;
                case "userEmail":
                    this.assignedRoles = _.orderBy(
                        this.assignedRoles,
                        assignedRole => [assignedRole.user.email, assignedRole.user.name, assignedRole.role.name],
                        sortDirection);
                    break;
            }
            this.lastSortedBy = sortBy;
        }

        submitRole() {
            if (this.selectedUser === null || angular.isUndefined(this.selectedUser)) {
                return;
            }
            if (this.selectedUser.id === null || angular.isUndefined(this.selectedUser.id)) {
                return;
            }

            var selectedRoleId = parseInt(this.selectedRoleIdAsString);
            var msg = this.notify.addInfoMessage("Tilføjer rolle");

            this.dataProcessingAgreementService.assignNewRole(this.dataProcessingAgreement.id, selectedRoleId, this.selectedUser.id)
                .then(
                    () => {
                        msg.toSuccessMessage("Rollen er tilføjet");
                        var assignedRole = this.getRoleFromId(selectedRoleId);
                        var user = <Models.ViewModel.Generic.Roles.IUserViewModel>{ id: this.selectedUser.optionalObjectContext.id, name: this.selectedUser.optionalObjectContext.name, email: this.selectedUser.optionalObjectContext.email }
                        var newAssignedRole = this.createEditableAssignedRole(user,
                            assignedRole.optionalObjectContext,
                            this.selectedUser,
                            assignedRole.id.toString());

                        this.assignedRoles.push(newAssignedRole);
                        this.selectedRoleIdAsString = this.roles[0].id.toString();
                        this.selectedUser = null;
                    },
                    (errorResponse: Models.Api.ApiResponseErrorCategory) => {
                        switch (errorResponse) {
                        case Models.Api.ApiResponseErrorCategory.Conflict:
                            msg.toErrorMessage("Fejl! Rollen er allerede tildelt den valgte bruger!");
                            break;
                        default:
                            msg.toErrorMessage("Fejl! Kunne ikke tilføje rolle!");
                            break;
                        }
                    });
        }

        removeRole(assignedRole: Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel) {
            var roleId = assignedRole.role.id;
            var userId = assignedRole.user.id;
            var msg = this.notify.addInfoMessage("Fjerner rolle");
            this.dataProcessingAgreementService.removeRole(
                this.dataProcessingAgreement.id, roleId, userId)
                .then(
                    () => {
                        msg.toSuccessMessage("Rollen er fjernet");
                        _.remove(this.assignedRoles, role => role === assignedRole);
                    },
                    (errorResponse: Models.Api.ApiResponseErrorCategory) => {
                        switch (errorResponse) {
                            default:
                                msg.toErrorMessage("Fejl! Kunne ikke fjerne rolle!");
                                break;
                        }
                    });
        }

        editRole(assignedRole: Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel) {
            var newRole = this.getRoleFromId(parseInt(assignedRole.newRoleIdAsString));

            if (angular.isUndefined(newRole)) {
                assignedRole.isEditing = false;
                return;
            }

            if (assignedRole.newUser.optionalObjectContext.id === assignedRole.user.id && newRole.optionalObjectContext.id === assignedRole.role.id) {
                assignedRole.isEditing = false;
                return;
            }

            var msg = this.notify.addInfoMessage("Retter rolle");

            this.dataProcessingAgreementService.assignNewRole(this.dataProcessingAgreement.id, newRole.id, assignedRole.newUser.id)
                .then(() => {
                    msg.toSuccessMessage("Den nye rolle er oprettet");
                    this.dataProcessingAgreementService.removeRole(this.dataProcessingAgreement.id, assignedRole.role.id, assignedRole.user.id)
                        .then(
                            () => {
                                msg.toSuccessMessage("Rollen er rettet");
                                _.remove(this.assignedRoles, role => role === assignedRole);
                            },
                            (errorResponse: Models.Api.ApiResponseErrorCategory) => {
                                switch (errorResponse) {

                                    default:
                                        msg.toErrorMessage("Fejl! Kunne ikke fjerne den gamle rolle!");
                                        break;
                                }
                            });

                    var newAssignedRole = this.createEditableAssignedRole(
                        assignedRole.newUser.optionalObjectContext,
                        newRole.optionalObjectContext,
                        assignedRole.newUser,
                        newRole.id.toString());
                    this.assignedRoles.push(newAssignedRole);
                },
                    (errorResponse: Models.Api.ApiResponseErrorCategory) => {
                        switch (errorResponse) {
                            case Models.Api.ApiResponseErrorCategory.Conflict:
                                msg.toErrorMessage("Fejl! Rollen er allerede tildelt den valgte bruger!");
                                break;
                            default:
                                msg.toErrorMessage("Fejl! Kunne ikke rette rolle!");
                                break;
                        }
                    });
        }

        private mapToEditableAssignedRole(input: Models.DataProcessing.IAssignedRoleDTO): Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel {
            const isExpired = !_.some(this.roles, role => role.id === input.role.id);
            input.role.name = this.formatRoleName(input.role, isExpired);
            const newUser = <Models.ViewModel.Generic.Select2OptionViewModel>{
                id: input.user.id,
                text: input.user.name,
                optionalObjectContext: input.user
            };
            return this.createEditableAssignedRole(input.user, input.role, newUser, input.role.id.toString());
        }

        private getRoleFromId(roleId: number) {
            return _.find(this.roles, role => role.id === roleId);
        }

        private formatRoleName(input: Models.DataProcessing.IDataProcessingRoleDTO, expired: boolean): string {
            return `${input.name} (${expired ? "udgået" : input.hasWriteAccess ? "skriv" : "læs"})`;
        }

        private mapToRoleOptions(input: Models.DataProcessing.IDataProcessingRoleDTO): Models.ViewModel.Generic.Select2OptionViewModel {
            const formattedRole = <Models.ViewModel.Generic.Roles.IRoleViewModel>{
                id: input.id,
                name: this.formatRoleName(input, false),
                note: input.note,
                hasWriteAccess: input.hasWriteAccess,
            };
            return <Models.ViewModel.Generic.Select2OptionViewModel>{
                id: input.id,
                text: this.formatRoleName(input, false),
                optionalObjectContext: formattedRole
            }
        }

        private formatResult(select2OptionViewModel: Models.ViewModel.Generic.Select2OptionViewModel) {
            var result = `<div>${select2OptionViewModel.text}</div>`;
            if (select2OptionViewModel.optionalObjectContext) {
                result += `<div class='small'>${select2OptionViewModel.optionalObjectContext.email}</div>`;
            }
            return result;
        }

        private createEditableAssignedRole(
            user: Models.ViewModel.Generic.Roles.IUserViewModel,
            role: Models.ViewModel.Generic.Roles.IRoleViewModel,
            newUser: Models.ViewModel.Generic.Select2OptionViewModel,
            newRoleIdAsString: string): Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel {
            var newAssignedRole = <Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel>{
                user: user,
                role: role,
                newUser: newUser,
                newRoleIdAsString: newRoleIdAsString
            };
            newAssignedRole.editUserOptions = this.getAvailableUserOptions(() => parseInt(newAssignedRole.newRoleIdAsString));
            return newAssignedRole;
        }

        private getAvailableUserOptions(getRoleId: () => number) {
            return this.select2LoadingService.loadSelect2WithDataSource(
                (query: string) =>
                this.dataProcessingAgreementService.getApplicableUsers(this.dataProcessingAgreement.id,
                    getRoleId(),
                    query)
                .then(
                    results =>
                    results
                    .map(result =>
                        <Models.ViewModel.Generic.Select2OptionViewModel>
                        {
                            id: result.id,
                            text: result.name,
                            optionalObjectContext: result
                        }),
                    _ => [])
                , false
                , this.formatResult);
        }

    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.edit-agreement.roles", {
                url: "/roles",
                templateUrl: "app/components/data-processing/tabs/data-processing-agreement-tab-roles.view.html",
                controller: EditRolesDataProcessingAgreementController,
                controllerAs: "vm",
                resolve: {
                    dataProcessingAgreementRoles: ["dataProcessingAgreementService", "dataProcessingAgreement",
                        (dataProcessingAgreementService, dataProcessingAgreement) => dataProcessingAgreementService.getAvailableRoles(dataProcessingAgreement.id)],
                    dataProcessingAgreement: [
                        "dataProcessingAgreementService", "$stateParams", (dataProcessingAgreementService: Services.DataProcessing.IDataProcessingAgreementService, $stateParams) => dataProcessingAgreementService.get($stateParams.id)
                    ]
                }
            });
        }]);
}
