﻿module Kitos.DataProcessing.Registration.Edit.Roles {
    "use strict";

    export class EditRolesDataProcessingRegistrationController {
        static $inject: Array<string> = [
            "dataProcessingRegistrationService",
            "notify",
            "hasWriteAccess",
            "dataProcessingRegistration",
            "dataProcessingRegistrationRoles",
            "select2LoadingService"
        ];

        headerName: string;
        roles: Models.ViewModel.Generic.Select2OptionViewModel<any>[];
        assignedRoles: Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel[];
        selectedUser: Models.ViewModel.Generic.Select2OptionViewModel<any>;
        userOptions: any;
        lastSortedBy: string;
        shouldSortReversed: boolean;
        rolesSelect2Config : any;
        newRoleSelect2Entry: Models.ViewModel.Generic.Select2OptionViewModel<Models.ViewModel.Generic.Roles.IRoleViewModel>;

        constructor(
            private readonly dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService,
            private readonly notify,
            public hasWriteAccess,
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO,
            dataProcessingRegistrationRoles: Models.DataProcessing.IDataProcessingRoleDTO[],
            private readonly select2LoadingService: Services.ISelect2LoadingService) {

            this.headerName = dataProcessingRegistration.name;
            this.roles = dataProcessingRegistrationRoles.map(role => this.mapToRoleOptions(role));
            this.rolesSelect2Config = select2LoadingService.select2LocalDataNoSearch(() => this.roles, false);
            this.lastSortedBy = "";
            this.shouldSortReversed = false;

            this.assignedRoles = this.dataProcessingRegistration.assignedRoles
                .map(assignedRole => this.mapToEditableAssignedRole(assignedRole));
            this.sortAssignedRoles("roleName"); // Initial sorting priority

            this.selectedUser = null;

            if (this.roles.length < 1) {
                this.newRoleSelect2Entry = null;
                this.userOptions = this.getAvailableUserOptions(() => null);
            } else {
                this.newRoleSelect2Entry = this.roles[0];
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

            this.dataProcessingRegistrationService.assignNewRole(this.dataProcessingRegistration.id, this.newRoleSelect2Entry.id, this.selectedUser.id)
                .then(
                    () => {
                        msg.toSuccessMessage("Rollen er tilføjet");
                        var assignedRole = this.newRoleSelect2Entry;
                        var user = <Models.ViewModel.Generic.Roles.IUserViewModel>{ id: this.selectedUser.optionalObjectContext.id, name: this.selectedUser.optionalObjectContext.name, email: this.selectedUser.optionalObjectContext.email }
                        var newAssignedRole = this.createEditableAssignedRole(user,
                            assignedRole.optionalObjectContext,
                            this.selectedUser,
                            assignedRole);

                        this.assignedRoles.push(newAssignedRole);
                        this.newRoleSelect2Entry = this.roles[0];
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
            this.dataProcessingRegistrationService.removeRole(
                this.dataProcessingRegistration.id, roleId, userId)
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
            var newRole = assignedRole.newRole;

            if (angular.isUndefined(newRole)) {
                assignedRole.isEditing = false;
                return;
            }

            if (assignedRole.newUser.optionalObjectContext.id === assignedRole.user.id && newRole.optionalObjectContext.id === assignedRole.role.id) {
                assignedRole.isEditing = false;
                return;
            }

            var msg = this.notify.addInfoMessage("Retter rolle");

            this.dataProcessingRegistrationService.assignNewRole(this.dataProcessingRegistration.id, newRole.id, assignedRole.newUser.id)
                .then(() => {
                    msg.toSuccessMessage("Den nye rolle er oprettet");
                    this.dataProcessingRegistrationService.removeRole(this.dataProcessingRegistration.id, assignedRole.role.id, assignedRole.user.id)
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
                        newRole);
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
            return this.newRoleSelect2Entry.optionalObjectContext.id;
        }

        private mapToEditableAssignedRole(input: Models.DataProcessing.IAssignedRoleDTO): Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel {
            input.role.name = this.formatRoleName(input.role);
            const isExpired = !_.some(this.roles, role => role.id === input.role.id);
            if (isExpired) {
                //Pre-format before r/w formatting
                input.role.name += " (udgået)";
            }
            const newUser = <Models.ViewModel.Generic.Select2OptionViewModel<Models.DataProcessing.ISimpleUserDTO>>{
                id: input.user.id,
                text: input.user.name,
                optionalObjectContext: input.user
            }; 

            return this.createEditableAssignedRole(input.user, input.role, newUser,  this.mapToRoleOptions(input.role, true));
        }

        private formatRoleName(input: Models.DataProcessing.IDataProcessingRoleDTO): string {
            return `${input.name} (${input.hasWriteAccess ? "skriv" : "læs"})`;
        }

        private mapToRoleOptions(input: Models.DataProcessing.IDataProcessingRoleDTO, nameIsFormatted? : boolean): Models.ViewModel.Generic.Select2OptionViewModel<Models.ViewModel.Generic.Roles.IRoleViewModel> {
            const name = nameIsFormatted ? input.name : this.formatRoleName(input);
            const formattedRole = <Models.ViewModel.Generic.Roles.IRoleViewModel>{
                id: input.id,
                name: name,
                note: input.note,
                hasWriteAccess: input.hasWriteAccess,
            };
            return <Models.ViewModel.Generic.Select2OptionViewModel<Models.ViewModel.Generic.Roles.IRoleViewModel>>{
                id: input.id,
                text: name,
                optionalObjectContext: formattedRole
            }
        }

        private sortAssignedRolesFunction(
            assignedRole1: Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel,
            assignedRole2: Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel,
            parameterSortPriorityList: string[],
            reversed: boolean) {
            const sortValue = reversed ? -1 : 1;

            for (let i = 0; i <= 2; i++) {
                const priorityProperties = parameterSortPriorityList[i].split(".");
                const lhs = assignedRole1[priorityProperties[0]][priorityProperties[1]];
                const rhs = assignedRole2[priorityProperties[0]][priorityProperties[1]];
                if (lhs !== rhs) {
                    return lhs > rhs
                        ? sortValue
                        : -sortValue;
                }
            }

            return -sortValue;
        }

        private formatResult(select2OptionViewModel: Models.ViewModel.Generic.Select2OptionViewModel<any>) {
            let result = `<div>${select2OptionViewModel.text}</div>`;
            if (select2OptionViewModel.optionalObjectContext) {
                result += `<div class='small'>${select2OptionViewModel.optionalObjectContext.email}</div>`;
            }
            return result;
        }

        private createEditableAssignedRole(
            user: Models.ViewModel.Generic.Roles.IUserViewModel,
            role: Models.ViewModel.Generic.Roles.IRoleViewModel,
            newUser: Models.ViewModel.Generic.Select2OptionViewModel<any>,
            newRole: Models.ViewModel.Generic.Select2OptionViewModel<Models.ViewModel.Generic.Roles.IRoleViewModel>): Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel {
            var newAssignedRole = <Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel>{
                user: user,
                role: role,
                newUser: newUser,
                newRole: newRole
            };
            newAssignedRole.editUserOptions = this.getAvailableUserOptions(() => newAssignedRole.newRole.optionalObjectContext.id);
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
                this.dataProcessingRegistrationService.getApplicableUsers(this.dataProcessingRegistration.id,
                    getRoleId(),
                    query)
                .then(
                    results =>
                    results
                        .map(result =>
                            <Models.ViewModel.Generic.Select2OptionViewModel<Models.DataProcessing.ISimpleUserDTO>>
                        {
                            id: result.id,
                            text: result.name,
                            optionalObjectContext: result,
                        }),
                    _ => [])
                , false
                , this.formatResult);
        }

    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.edit-registration.roles", {
                url: "/roles",
                templateUrl: "app/components/data-processing/tabs/data-processing-registration-tab-roles.view.html",
                controller: EditRolesDataProcessingRegistrationController,
                controllerAs: "vm",
                resolve: {
                    dataProcessingRegistrationRoles: ["dataProcessingRegistrationOptions",
                        (dataProcessingRegistrationOptions: Models.DataProcessing.IDataProcessingRegistrationOptions) => dataProcessingRegistrationOptions.roles],
                    dataProcessingRegistration: [
                        "dataProcessingRegistrationService", "$stateParams", (dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService, $stateParams) => dataProcessingRegistrationService.get($stateParams.id)
                    ]
                }
            });
        }]);
}
