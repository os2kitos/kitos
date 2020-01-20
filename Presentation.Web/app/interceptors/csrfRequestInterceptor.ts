
((ng, app) => {
    app.factory("csrfRequestInterceptor", ["$cookies", "$injector", "$q", ($cookies: ng.cookies.ICookiesService, $injector: ng.auto.IInjectorService, $q: ng.IQService) => ({
        request(config) {
            if (config.method === "GET") {
                return config;
            }

            var getHiddenFieldValue = () => {
                return angular.element("input[id='__RequestVerificationToken']").val();
            }

            var setHiddenFieldValue = (updatedHiddenValue: string) => {
                (document.getElementById("__RequestVerificationToken") as HTMLInputElement).value = updatedHiddenValue;
            }

            var shouldUpdate = () => {
                const cookie = $cookies.get("XSRF-TOKEN");
                return cookie == null || getHiddenFieldValue() === "";
            }

            var prepareConfig = (requestConfig: any) => {
                requestConfig.headers["X-XSRF-TOKEN"] = getHiddenFieldValue();
                return requestConfig;
            }

            if (shouldUpdate()) {
                const $http = $injector.get("$http");

                return $http
                    .get("api/authorize/antiforgery")
                    .then(response => setHiddenFieldValue(response.data.toString()))
                    .then(() => prepareConfig(config));
            } else {
                return prepareConfig(config);
            }
        }
    })]);

})(angular, app);