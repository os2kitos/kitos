module Kitos.Services {

    export interface IUserRoleAdministrationService {
        getAssignedRoles(organizationId: number, userId: number): ng.IPromise<Models.Users.UserRoleAssigmentDTO>
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
                                case 3:
                                    mappedRole = Models.OrganizationRole.ProjectModuleAdmin;
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

        static $inject = ["genericApiWrapper", "apiUseCaseFactory"];

        constructor(
            private readonly genericApiWrapper: Services.Generic.ApiWrapper,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory) { }
    }

    app.service("userRoleAdministrationService", UserRoleAdministrationService);
}