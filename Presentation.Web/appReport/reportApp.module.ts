var app = angular.module("reportApp", [
    "ui.router",
    "ui.bootstrap",
    "ngAnimate",
    "ngSanitize",
    "notify",
    "ngCookies"]);

app.constant("$", $)
    .constant("_", _);

app.config([
    "$httpProvider",
    ($httpProvider) => {

        $httpProvider.defaults.headers.get = {
            "Cache-Control": "no-cache"
        };

        //Disable built-in xsrf in angular - it overrides our interceptor
        $httpProvider.defaults.xsrfCookieName = "IGNORED-XSRF-TOKEN";
        $httpProvider.defaults.xsrfHeaderName = "IGNORED-XSRF-TOKEN";

        $httpProvider.interceptors.push("csrfRequestInterceptor");

    }
]);