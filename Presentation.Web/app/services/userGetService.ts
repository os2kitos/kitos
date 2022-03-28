module Kitos.Services {
    "use strict";


    export class UserGetService {

        public static $inject: string[] = ["$http"];

        constructor(private $http: IHttpServiceWithCustomConfig) {
        }

        GetAllUsers = () => {
            return this.$http.get<Models.IUser>(`odata/Users`);
        }

        /**
         * Returns a list of users filtered by organizationId, ordered by Name asc, LastName asc
         * @param id
         */
        GetAllUsersFromOrganizationById = (id : number) => {
            return this.$http.get<Models.IUser>(`odata/Users?$filter=OrganizationRights/any(o:o/OrganizationId eq (${id}))&$orderby=Name,LastName`);
        }
    }

    app.service("UserGetService", UserGetService);
}
