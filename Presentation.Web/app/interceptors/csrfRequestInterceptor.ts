
((ng, app) => {
    app.factory("csrfRequestInterceptor", () => ({
        request(config) {
            if (config.method === "GET") {
                return config;
            }

            //TODO CSRF: Check for available cookie. If expired make new request to api/authorize/antiforgery

            config.headers["X-XSRF-TOKEN"] = angular.element("input[id='__RequestVerificationToken']").val();

            return config;
        },
        response(response) {

            if (response.config.url === "api/authorize/antiforgery") {
                (document.getElementById("__RequestVerificationToken") as HTMLInputElement).value = response.data;
            }

            return response;
        }
    }));

})(angular, app);