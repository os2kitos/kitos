module Kitos.Services {
    export class ModifyingHttp {
        static $inject = ["$http"];

        constructor(private readonly $http: ng.IHttpService, private config: ng.IRequestShortcutConfig) {

        }

        private csrfHeader = {
            headers: {
                "X-XSRF-Token": $("input[name=__RequestVerificationToken]")
            }
        }

        post<T>(route, content) {
            return this.$http.post<T>(route, content, this.csrfHeader);
        }
    }
}
