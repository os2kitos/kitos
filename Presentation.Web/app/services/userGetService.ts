module Kitos.Services {
    "use strict";


    //laver kaldet til odata for at hente data kollektions ud af databasen
    export class UserGetService {

        public static $inject: string[] = ["$http"];

        constructor(private $http: IHttpServiceWithCustomConfig) {
        }
        GetAllUsers = () => {
            return this.$http.get<Models.IUser>(`odata/Users`);
        }

        GetAllUsersFromOrganizationById = (id : number) => {
            return this.$http.get<Models.IUser>(`odata/Users?$filter=OrganizationRights/any(o:o/OrganizationId eq (${id}))`);
        }
    }

    app.service("UserGetService", UserGetService);
}
