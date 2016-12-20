module Kitos.Services {
    "use strict";

    export class itSystemUsageService {

        public static $inject: string[] = ["$http", "notify"];
        private baseUrl = "/odata/ItSystemUsages";

        constructor(private $http: IHttpServiceWithCustomConfig, private notify) {
        }

        patchSystem = (id: number, payload: any) => {
            this.$http.patch(this.baseUrl + `(${id})`, payload)
                .then(() => {
                this.notify.addSuccessMessage("Feltet er opdateret!");
            },
                () => this.notify.addErrorMessage("Fejl! Feltet kunne ikke opdateres!"));
        }
    }

    app.service("itSystemUsageService", itSystemUsageService);
}
