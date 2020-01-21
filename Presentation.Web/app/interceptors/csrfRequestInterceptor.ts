﻿
((ng, app) => {
    app.factory("csrfRequestInterceptor", ["$cookies", "$injector", "$q", ($cookies: ng.cookies.ICookiesService, $injector: ng.auto.IInjectorService, $q: ng.IQService) => ({
        request(config) {

            const isNonMutating = (requestConfig: any) => {
                return requestConfig.method === "GET" || requestConfig.method === "HEAD" || requestConfig.method === "OPTIONS";
            };

            var getHiddenFieldValue = () : string => {
                return angular.element(`input[id='${Kitos.Constants.CSRF.HiddenFieldName}']`).val();
            };

            var setHiddenFieldValue = (updatedHiddenValue: string) => {
                (document.getElementById(Kitos.Constants.CSRF.HiddenFieldName) as HTMLInputElement).value = updatedHiddenValue;
            }

            const shouldUpdate = () => {
                const cookie = $cookies.get(Kitos.Constants.CSRF.CSRFCookie);
                return cookie == null || getHiddenFieldValue() === "";
            };

            var prepareConfig = (requestConfig: any) => {
                requestConfig.headers[Kitos.Constants.CSRF.CSRFHeader] = getHiddenFieldValue();
                return requestConfig;
            }

            if (isNonMutating(config)) {
                return config;
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