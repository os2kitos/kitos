module Kitos.DataProcessing.Agreement.Edit.Roles {
    "use strict";

    export class EditRolesDataProcessingAgreementController {
        static $inject: Array<string> = [
            "dataProcessingAgreementService",
            "notify",
            "hasWriteAccess",
            "dataProcessingAgreement",
            "dataProcessingAgreementRoles",
            "$http"
        ];

        constructor(
            private dataProcessingAgreementService: Services.DataProcessing.IDataProcessingAgreementService,
            private notify,
            public hasWriteAccess,
            private dataProcessingAgreement: Models.DataProcessing.IDataProcessingAgreementDTO,
            private dataProcessingAgreementRoles: Models.DataProcessing.IDataProcessingRoleDTO[],
            private $http) {
        }

        headerName = this.dataProcessingAgreement.name;

        roles = _.map(this.dataProcessingAgreementRoles, this.mapToRoleOptions);

        selectedRoleIdAsString:string = this.roles[0].id.toString();

        selectedUser: Models.ViewModel.Generic.Roles.IUserOptionsViewModel;

        mapToRoleOptions(input: Models.DataProcessing.IDataProcessingRoleDTO): Models.ViewModel.Generic.Roles.IRoleOptionsViewModel {
            return {
                id: input.id,
                text: `${input.name} ${input.hasWriteAccess ? "skriv" : "læs"}`,
                role: input
            }
        }

        getUsers(queryParams) {
            return this.$http.get(`api/v1/data-processing-agreement/${this.dataProcessingAgreement.id}/available-roles/${this.selectedRoleIdAsString}/applicable-users?nameContent=${queryParams.data.query}`)
                .then(queryParams.success, () => null);
        }

        formatResult(obj) {
            var result = "<div>" + obj.text + "</div>";

            //obj.user might contain more info about the user
            if (obj.user) {
                result += "<div class='small'>" + obj.user.email + "</div>";
            }

            return result;
        }

        getOptions() {
            var self = this;
            return {
                formatResult: self.formatResult,
                minimumInputLength: 1,
                initSelection(elem, callback) {
                },
                allowClear: true,
                ajax: {
                    data(term, page) {
                        return { query: term };
                    },
                    quietMillis: 500,
                    transport(queryParams) {
                        const res = self.getUsers(queryParams);

                        return res;
                    },

                    results(data, page) {
                        var results = [];
                        _.each(data.data.response, (obj) => {
                            var userOption = {
                                id: obj.id,
                                text: obj.name,
                                user: obj
                            }
                            results.push(userOption);
                        });
                        return { results: results };
                    }
                }
            };
        }

        userOptions = this.getOptions();

        assignedRoles = _.map(this.dataProcessingAgreement.assignedRoles, this.mapToEditableAssignedRole);

        mapToEditableAssignedRole(input: Models.DataProcessing.IAssignedRoleDTO): Models.ViewModel.Generic.Roles.IEditableAssignedRoleViewModel {
            return {
                user: input.user,
                role: input.role,
                newUser: {
                    id: input.user.id,
                    text: input.user.name,
                    user: input.user
                } as Models.ViewModel.Generic.Roles.IUserOptionsViewModel,
                newRoleIdAsString: input.role.id.toString(),
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
                        var user = { id: this.selectedUser.user.id, name: this.selectedUser.user.name, email: this.selectedUser.user.email}
                        var newAssignedRole = {
                            user: user,
                            role: assignedRole.role,
                            newUser: {
                                id: user.id,
                                text: user.name,
                                user: user
                            } as Models.ViewModel.Generic.Roles.IUserOptionsViewModel,
                            newRoleIdAsString: assignedRole.role.id.toString(),
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

            if (assignedRole.newUser.user === assignedRole.user && newRole.role === assignedRole.role) {
                msg.toErrorMessage("Fejl! Kunne ikke rette rolle!");
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
                        user: assignedRole.newUser.user,
                        role: newRole.role,
                        newUser: assignedRole.newUser,
                        newRoleIdAsString: newRole.id.toString()
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
