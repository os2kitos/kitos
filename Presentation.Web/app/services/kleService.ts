module Kitos.Services {
    "use strict";

    export class KLEservice
    {
        public static $inject: string[] = ["$http"];

        constructor(private $http: ng.IHttpService) {
        }
        
        getStatus() {
            const url = `api/kle`;
            return this.$http({ method: "GET", url: url, });
        }

    }
    app.service("KLEservice", KLEservice);
}