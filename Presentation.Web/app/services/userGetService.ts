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
         * TODO: Add comment regarging what order to expect the results in
         * @param id
         */
        GetAllUsersFromOrganizationById = (id : number) => {
            return this.$http.get<Models.IUser>(`odata/Users?$filter=OrganizationRights/any(o:o/OrganizationId eq (${id}))&$orderby=Name,LastName`);
        }
    }

    app.service("UserGetService", UserGetService);
}
