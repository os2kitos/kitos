module Kitos.Services {

    export interface IUserRoleAdministrationService {
        getAssignedRoles(organizationId: number, userId: number): ng.IPromise<Models.Users.UserRoleAssigmentDTO>;
        removeAssignedRoles(organizationId: number, userId: number, rolesToRemove: Models.Users.UserRoleAssigmentDTO): ng.IPromise<boolean>;
        transferAssignedRoles(organizationId: number, userId: number, toUserId: number, rolesToTransfer: Models.Users.UserRoleAssigmentDTO): ng.IPromise<boolean>;
        removeUser(organizationId: number, userId: number): ng.IPromise<boolean>;
    }

    class UserRoleAdministrationService implements IUserRoleAdministrationService {
        private getBaseUri(organizationId: number, userId: number, additionalSegments: string = "") {
            return `api/v1/organizations/${organizationId}/users/${userId}/roles${additionalSegments}`;
        }

        getAssignedRoles(organizationId: number, userId: number): angular.IPromise<Models.Users.UserRoleAssigmentDTO> {
            return this.genericApiWrapper
                .getDataFromUrl<Models.Users.UserRoleAssigmentDTO>(this.getBaseUri(organizationId, userId))
                .then(result => {

                    const mappedRoles = new Array<Models.OrganizationRole>();
                    for (let role of result.administrativeAccessRoles) {
                        if (typeof (role) === "number") {
                            let mappedRole: Models.OrganizationRole | null = null;
                            switch (role) {
                                case 1:
                                    mappedRole = Models.OrganizationRole.LocalAdmin;
                                    break;
                                case 2:
                                    mappedRole = Models.OrganizationRole.OrganizationModuleAdmin;
                                    break;
                                case 4:
                                    mappedRole = Models.OrganizationRole.SystemModuleAdmin;
                                    break;
                                case 5:
                                    mappedRole = Models.OrganizationRole.ContractModuleAdmin;
                                    break;
                                case 8:
                                    mappedRole = Models.OrganizationRole.RightsHolderAccess;
                                    break;
                                default:
                                    //Ignored so not part of the final result
                                    break;

                            }
                            if (mappedRole !== null) {
                                mappedRoles.push(mappedRole);
                            }
                        } else {
                            mappedRoles.push(role);
                        }
                    }
                    result.administrativeAccessRoles = mappedRoles;
                    return result;
                });

        }

        removeAssignedRoles(organizationId: number, userId: number, rolesToRemove: Models.Users.UserRoleAssigmentDTO): angular.IPromise<boolean> {
            return this.apiUseCaseFactory
                .createDeletion("Rettigheder",
                    () => this.genericApiWrapper.delete(this.getBaseUri(organizationId, userId, "/range"),
                        {
                            adminRoles: rolesToRemove.administrativeAccessRoles,
                            businessRights: rolesToRemove.rights
                        }))
                .executeAsync();
        }

        removeUser(organizationId: number, userId: number): angular.IPromise<boolean> {
            return this.apiUseCaseFactory
                .createDeletion("Brugeren", () => this.genericApiWrapper.delete(this.getBaseUri(organizationId, userId)))
                .executeAsync();
        }

        transferAssignedRoles(organizationId: number,
            userId: number,
            toUserId: number,
            rolesToTransfer: Models.Users.UserRoleAssigmentDTO): angular.IPromise<boolean> {
            return this.apiUseCaseFactory
                .createUpdate("Rettigheder",
                    () => this.genericApiWrapper.patch(this.getBaseUri(organizationId, userId, "/range/transfer"),
                        {
                            toUserId: toUserId,
                            adminRoles: rolesToTransfer.administrativeAccessRoles,
                            businessRights: rolesToTransfer.rights
                        }))
                .executeAsync()
                .then(() => true, () => false);
        }

        static $inject = ["genericApiWrapper", "apiUseCaseFactory"];

        constructor(
            private readonly genericApiWrapper: Services.Generic.ApiWrapper,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory) { }
    }

    app.service("userRoleAdministrationService", UserRoleAdministrationService);
}