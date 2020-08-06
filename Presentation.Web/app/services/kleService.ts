module Kitos.Services {
    "use strict";

    export class KLEservice
    {
        public static $inject: string[] = ["$http", "userService"];

        constructor(private $http: ng.IHttpService, private userService : IUserService) {
        }
        
        getStatus() {
            const url = `api/v1/kle/status`;
            return this.$http({ method: "GET", url: url, });
        }

        getChanges() {
            const url = `api/v1/kle/changes`;
            return this.$http({ method: "GET", url: url,});
        }

        applyUpdateKLE() {
            return this
                .userService
                .getUser()
                .then((user: IUser) =>
                    this.$http({ method: "PUT", url: `api/v1/kle/update?organizationId=${user.id}` }));
        }

    }
    app.service("kleService", KLEservice);
}