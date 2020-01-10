module Kitos.Services {
    "use strict";

    export class KLEservice
    {
        public static $inject: string[] = ["$http"];

        constructor(private $http: ng.IHttpService) {
        }
        
        getStatus() {
            const url = `api/v1/kle/status`;
            return this.$http({ method: "GET", url: url, });
        }

        getChanges() {
            const url = `api/v1/kle/changes`;
            return this.$http({ method: "GET", url: url,});
        }

        updateKLE() {
            const url = `api/kle`;
            return this.$http({ method: "GET", url: url, });
        }

    }
    app.service("KLEservice", KLEservice);
}