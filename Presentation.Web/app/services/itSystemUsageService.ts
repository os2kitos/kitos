module Kitos.Services {
    "use strict";

    export class itSystemUsageService {

        public static $inject: string[] = ["$http"];
        private baseUrl = "/odata/ItSystemUsages";

        constructor(private $http: IHttpServiceWithCustomConfig) {
        }

        patchSystem = (id: number, payload: any) => {
            this.$http.patch(this.baseUrl + `(${id})`, payload);
        }
    }

    app.service("itSystemUsageService", itSystemUsageService);
}
