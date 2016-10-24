module Kitos.Services {
    "use strict";

    export class OrganizationService {

        public static $inject: string[] = ["$http"];

        constructor(private $http: IHttpServiceWithCustomConfig) {
        }

        public addRole(organizationId: number, user: Services.IUser, role: Models.OrganizationRole): ng.IHttpPromise<Models.IOrganizationRight> {
            var rightsPayload: Models.IOrganizationRight = {
                UserId: user.id,
                ObjectOwner: user,
                ObjectOwnerId: user.id,
                Role: role
            };

            return this.$http.post<Models.IOrganizationRight>(`odata/Organizations(${organizationId})/Rights`, rightsPayload);
        }

    }

    app.service("organizationService", OrganizationService);
}
