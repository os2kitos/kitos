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

        constructor(
            private dataProcessingAgreementService: Services.DataProcessing.IDataProcessingAgreementService,
            private notify,
            public hasWriteAccess,
            private dataProcessingAgreement: Models.DataProcessing.IDataProcessingAgreementDTO,
            private dataProcessingAgreementRoles: Models.DataProcessing.IDataProcessingRoleDTO[],
            private select2LoadingService: Services.ISelect2LoadingService) {
        }

        headerName = this.dataProcessingAgreement.name;

        roles = _.map(this.dataProcessingAgreementRoles, this.mapToRoleOptions);

        selectedRoleIdAsString: string = this.roles[0].id.toString();

        selectedUser: Models.ViewModel.Generic.Select2OptionViewModel;

        mapToRoleOptions(input: Models.DataProcessing.IDataProcessingRoleDTO): Models.ViewModel.Generic.Select2OptionViewModel {
            return {
                id: input.id,
                text: `${input.name} (${input.hasWriteAccess ? "skriv" : "læs"})`,
                optionalObjectContext: input
            }
        }

        formatResult(select2OptionViewModel: Models.ViewModel.Generic.Select2OptionViewModel) {
            var result = `<div>${select2OptionViewModel.text}</div>`;
            if (select2OptionViewModel.optionalObjectContext) {
                result += `<div class='small'>${select2OptionViewModel.optionalObjectContext.email}</div>`;
            }
            return result;
        }

        userOptions = this.select2LoadingService.loadSelect2WithDataSource(
            (query: string) =>
                this.dataProcessingAgreementService.getApplicableUsers(this.dataProcessingAgreement.id,
                    parseInt(this.selectedRoleIdAsString),
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

        getEditUserOptions(roleId: number) {
            return this.select2LoadingService.loadSelect2WithDataSource(
                (query: string) =>
                this.dataProcessingAgreementService.getApplicableUsers(this.dataProcessingAgreement.id,
                    roleId,
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

        assignedRoles = _.map(this.dataProcessingAgreement.assignedRoles, assignedRole => this.mapToEditableAssignedRole(assignedRole, this));

        mapToEditableAssignedRole(input: Models.DataProcessing.IAssignedRoleDTO, self: EditRolesDataProcessingAgreementController): Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel {
            return {
                user: input.user,
                role: input.role,
                newUser: {
                    id: input.user.id,
                    text: input.user.name,
                    optionalObjectContext: input.user
                } as Models.ViewModel.Generic.Select2OptionViewModel,
                newRoleIdAsString: input.role.id.toString(),
                editUserOptions:  self.getEditUserOptions(input.role.id)
            } as Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel;
        }

        getRoleFromId(roleId: number) {
            return _.find(this.roles, role => role.id === roleId);
        }

        submitRole() {
            if (this.selectedUser.id === null || angular.isUndefined(this.selectedUser.id)) {
                return;
            }

            var selectedRoleId = parseInt(this.selectedRoleIdAsString);

            var msg = this.notify.addInfoMessage("Tilføjer rolle");

            this.dataProcessingAgreementService.assignNewRole(
                this.dataProcessingAgreement.id,
                selectedRoleId,
                this.selectedUser.id)
                .then(
                    () => {
                        msg.toSuccessMessage("Rollen er tilføjet");
                        var assignedRole = this.getRoleFromId(selectedRoleId);
                        var user = { id: this.selectedUser.optionalObjectContext.id, name: this.selectedUser.optionalObjectContext.name, email: this.selectedUser.optionalObjectContext.email }
                        var newAssignedRole = {
                            user: user,
                            role: assignedRole.optionalObjectContext,
                            newUser: this.selectedUser,
                            newRoleIdAsString: assignedRole.id.toString(),
                            editUserOptions: this.getEditUserOptions(assignedRole.id),
                        } as Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel;

                        this.assignedRoles.push(newAssignedRole);

                        this.selectedRoleIdAsString = this.roles[0].id.toString();
                        this.selectedUser = null;
                    },
                    (errorResponse: Models.Api.ApiResponseErrorCategory) => {
                        switch (errorResponse) {

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
                this.dataProcessingAgreement.id,
                roleId,
                userId)
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

            var msg = this.notify.addInfoMessage("Retter rolle");

            var newRole = this.getRoleFromId(parseInt(assignedRole.newRoleIdAsString));

            if (assignedRole.newUser.optionalObjectContext.id === assignedRole.user.id && newRole.optionalObjectContext.id === assignedRole.role.id) {
                msg.toSuccessMessage("Rolle ikke opdateret da samme rolle og bruger er valgt");
                assignedRole.isEditing = false;
                return;
            }

            this.dataProcessingAgreementService.assignNewRole(
                this.dataProcessingAgreement.id,
                newRole.id,
                assignedRole.newUser.id)
                .then(() => {
                    msg.toSuccessMessage("Den nye rolle er oprettet");
                    this.dataProcessingAgreementService.removeRole(
                        this.dataProcessingAgreement.id,
                        assignedRole.role.id,
                        assignedRole.user.id)
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

                    var newAssignedRole = {
                        user: assignedRole.newUser.optionalObjectContext,
                        role: newRole.optionalObjectContext,
                        newUser: assignedRole.newUser,
                        newRoleIdAsString: newRole.id.toString(),
                        editUserOptions: this.getEditUserOptions(newRole.id),
                    } as Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel;

                    this.assignedRoles.push(newAssignedRole);
                },
                    (errorResponse: Models.Api.ApiResponseErrorCategory) => {
                        switch (errorResponse) {

                            default:
                                msg.toErrorMessage("Fejl! Kunne ikke rette rolle!");
                                break;
                        }
                    });
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
                        (dataProcessingAgreementService, dataProcessingAgreement) => dataProcessingAgreementService.getAvailableRoles(dataProcessingAgreement.id)]
                }
            });
        }]);
}
