module Kitos.DataProcessing.Agreement.Edit.Roles {
    "use strict";

    export class EditRolesDataProcessingAgreementController {
        static $inject: Array<string> = [
            "dataProcessingAgreementService",
            "notify",
            "hasWriteAccess",
            "dataProcessingAgreement",
            "dataProcessingAgreementRoles",
            "select2LoadingService"
        ];

        headerName: string;
        roles: Models.ViewModel.Generic.Select2OptionViewModel[];
        assignedRoles: Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel[];
        selectedRoleIdAsString: string;
        selectedUser: Models.ViewModel.Generic.Select2OptionViewModel;
        userOptions: any;
        lastSortedBy: string;
        shouldSortReversed: boolean;

        constructor(
            private readonly dataProcessingAgreementService: Services.DataProcessing.IDataProcessingAgreementService,
            private readonly notify,
            public hasWriteAccess,
            private readonly dataProcessingAgreement: Models.DataProcessing.IDataProcessingAgreementDTO,
            dataProcessingAgreementRoles: Models.DataProcessing.IDataProcessingRoleDTO[],
            private readonly select2LoadingService: Services.ISelect2LoadingService) {

            this.headerName = dataProcessingAgreement.name;
            this.roles = dataProcessingAgreementRoles.map(role => this.mapToRoleOptions(role));

            this.lastSortedBy = "";
            this.shouldSortReversed = false;

            this.assignedRoles = this.dataProcessingAgreement.assignedRoles
                .map(assignedRole => this.mapToEditableAssignedRole(assignedRole));
            this.sortAssignedRoles("roleName"); // Initial sorting priority

            this.selectedUser = null;

            if (this.roles.length < 1) {
                this.selectedRoleIdAsString = null;
                this.userOptions = this.getAvailableUserOptions(() => null);
            } else {
                this.selectedRoleIdAsString = this.roles[0].id.toString();
                this.userOptions = this.getAvailableUserOptions(() => this.getSelectedRoleId());
            }
        }

        sortAssignedRoles(sortBy: string) {
            sortBy === this.lastSortedBy
                ? this.shouldSortReversed = !this.shouldSortReversed
                : this.shouldSortReversed = false;
            switch (sortBy) {
                case "roleName":
                    this.assignedRoles = this.assignedRoles
                        .sort((assignedRole1, assignedRole2) => this.sortAssignedRolesFunction(assignedRole1,
                            assignedRole2,
                            ["role.name", "user.name", "user.email"],
                            this.shouldSortReversed));
                    break;
                case "userName":
                    this.assignedRoles = this.assignedRoles
                        .sort((assignedRole1, assignedRole2) => this.sortAssignedRolesFunction(assignedRole1,
                            assignedRole2,
                            ["user.name", "user.email", "role.name"],
                            this.shouldSortReversed));
                    break;
                case "userEmail":
                    this.assignedRoles = this.assignedRoles
                        .sort((assignedRole1, assignedRole2) => this.sortAssignedRolesFunction(assignedRole1,
                            assignedRole2,
                            ["user.email", "user.name", "role.name"],
                            this.shouldSortReversed));
                    break;
            }
            this.lastSortedBy = sortBy;
        }

        submitRole() {
            if (this.checkSelectedUserDefined()) {
                return;
            }
            if (this.checkSelectedUserHasIdDefined()) {
                return;
            }

            var msg = this.notify.addInfoMessage("Tilføjer rolle");

            this.dataProcessingAgreementService.assignNewRole(this.dataProcessingAgreement.id, this.getSelectedRoleId(), this.selectedUser.id)
                .then(
                    () => {
                        msg.toSuccessMessage("Rollen er tilføjet");
                        var assignedRole = this.getRoleFromId(this.selectedRoleIdAsString);
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
            var newRole = this.getRoleFromId(assignedRole.newRoleIdAsString);

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

        private checkSelectedUserDefined() {
            return this.selectedUser === null || angular.isUndefined(this.selectedUser);
        }

        private checkSelectedUserHasIdDefined() {
            return this.selectedUser.id === null || angular.isUndefined(this.selectedUser.id);
        }

        private getSelectedRoleId() {
            return parseInt(this.selectedRoleIdAsString);
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

        private getRoleFromId(roleIdAsString: string) {
            var roleId = parseInt(roleIdAsString);
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

        private sortAssignedRolesFunction(
            assignedRole1: Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel,
            assignedRole2: Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel,
            parameterSortPriorityList: string[],
            reversed: boolean) {
            var sortValue = reversed ? -1 : 1;
            let priorityProperties = parameterSortPriorityList[0].split(".");
            if (assignedRole1[priorityProperties[0]][priorityProperties[1]] !== assignedRole2[priorityProperties[0]][priorityProperties[1]]) {
                return assignedRole1[priorityProperties[0]][priorityProperties[1]] > assignedRole2[priorityProperties[0]][priorityProperties[1]]
                    ? sortValue
                    : -sortValue;
            }

            priorityProperties = parameterSortPriorityList[1].split(".");
            if (assignedRole1[priorityProperties[0]][priorityProperties[1]] !== assignedRole2[priorityProperties[0]][priorityProperties[1]]) {
                return assignedRole1[priorityProperties[0]][priorityProperties[1]] > assignedRole2[priorityProperties[0]][priorityProperties[1]]
                    ? sortValue
                    : -sortValue;
            }

            priorityProperties = parameterSortPriorityList[2].split(".");
            return assignedRole1[priorityProperties[0]][priorityProperties[1]] > assignedRole2[priorityProperties[0]][priorityProperties[1]]
                ? sortValue
                : -sortValue;
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
            if (getRoleId() === null) {
                return {
                    data: () => ({ "results": [] }),
                    allowClear: true
                };
            }
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
