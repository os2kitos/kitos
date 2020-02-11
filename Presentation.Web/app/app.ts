var app = angular.module("app", [
    "ui.router",
    "ui.bootstrap",
    "ui.select2",
    "ngAnimate",
    "notify",
    "angularjs-dropdown-multiselect",
    "ngSanitize",
    "kendo.directives",
    "angular-loading-bar",
    "angular-confirm",
    "ui.bootstrap.tpls",
    "ngMessages",
    "ui.tree",
    "ui.tinymce",
    "oidc-angular",
    "ngCookies"]);

app.constant("JSONfn", JSONfn)
    .constant("moment", moment)
    .constant("$", $)
    .constant("_", _);

app.config([
    "$urlRouterProvider", ($urlRouterProvider: angular.ui.IUrlRouterProvider) => {
        $urlRouterProvider.otherwise("/");
    }
]);

app.config(["$authProvider", $authProvider => {

    $authProvider.configure({
        redirectUri: location.origin + "/#/?",
        scope: "openid email",
        basePath: location.origin.indexOf("test") > 0 ? "https://os2sso-test.miracle.dk" : "https://os2sso.miracle.dk",
        clientId: "kitos_client"
    });
}]);

app.config([
    "$httpProvider",
    "$windowProvider",
    "notifyProvider",
    ($httpProvider, $windowProvider, notifyProvider) => {
        $httpProvider.interceptors.push("httpBusyInterceptor");
        // for some reason templates aren't updated so this is needed
        $httpProvider.defaults.headers.get = {
            "Cache-Control": "no-cache"
        };

        //Disable built-in xsrf in angular - it overrides our interceptor
        $httpProvider.defaults.xsrfCookieName = "IGNORED-XSRF-TOKEN";
        $httpProvider.defaults.xsrfHeaderName = "IGNORED-XSRF-TOKEN";


        function isRunningOnHost(partialHostName) {
            return window.location.hostname.indexOf(partialHostName) !== -1;
        }

        //Configure notifications - use lower ttl on integration environment
        var ttl = (isRunningOnHost("kitos-integration") || isRunningOnHost("localhost")) ? 500 : 5000;
        notifyProvider.globalTimeToLive(ttl);
        notifyProvider.onlyUniqueMessages(false);

        // $window isn't ready yet, so fetch it ourself
        var $window = $windowProvider.$get();

        $httpProvider.interceptors.push("csrfRequestInterceptor");

        // encode all url requests - fixes IE not correctly encoding special chars
        $httpProvider.interceptors.push(() => ({
            request(config) {
                config.url = $window.encodeURI(config.url);
                return config;
            }
        }));
    }
]);
app.config([
    '$compileProvider',
    $compileProvider => {
        $compileProvider.aHrefSanitizationWhitelist(/^\s*(https?|ftp|mailto|chrome-extension|.*):/);
        // Angular before v1.2 uses $compileProvider.urlSanitizationWhitelist(...)
    }
]);

app.run([
    "$rootScope", "$http", "$state", "$uibModal", "notify", "userService", "uiSelect2Config", "navigationService", "$timeout", "$", "needsWidthFixService", "$cookies",
    ($rootScope, $http, $state, $modal, notify, userService, uiSelect2Config, navigationService, $timeout, $, needsWidthFixService, $cookies) => {

        // init info
        $rootScope.page = {
            title: "Index",
            subnav: []
        };

        $(window).resize(() => {
            $rootScope.positionSubnav();
        });

        $rootScope.positionSubnav = () => {
            $(document).ready(function () {
                $timeout(() => {
                    if ($rootScope.subnavPositionCenter) {
                        $("#subnav").css("text-align", "center");
                        $("#subnav").css("padding-left", "0");
                    } else {
                        if (typeof ($("#navbar-top a.active").offset()) === 'undefined') {
                            $timeout(() => {
                                $rootScope.positionSubnav();
                            }, 100);
                        } else {
                            const buttonWidth = $("#navbar-top a.active").width();
                            const distanceFromContainerToButton = $("#navbar-top").offset().left - $("#navbar-top a.active").offset().left;
                            const ulWidth = $("#subnav ul").width();
                            const subnavWidth = $("#navbar-top").width();
                            $("#subnav").css("text-align", "left");
                            $("#subnav").css("padding-left", `${((distanceFromContainerToButton * (-1)) - (ulWidth / 2) + (buttonWidth / 2)) / subnavWidth * 100}%`);
                        }
                    }

                    $rootScope.subnavNotPositioned = false;
                }, 0);
            });
        }

        // hide cancel button on login form unless the user is changing organization
        $rootScope.changingOrganization = false;

        $rootScope.$state = $state;

        // this will try to authenticate - to see if the user's already logged in
        userService.reAuthorize();

        uiSelect2Config.dropdownAutoWidth = true;

        // logout function for top navigation bar
        $rootScope.logout = () => {
            userService.logout().then(() => {
                $rootScope.changingOrganization = false;
                $state.go("index");
                $cookies.remove(Kitos.Constants.CSRF.CSRFCookie);
            });
        };

        // changeOrganization function for top navigation bar
        $rootScope.changeOrganization = () => {

            $rootScope.changingOrganization = true;

            userService.changeOrganization().then((user) => {

                if (navigationService.checkState(user.defaultUserStartPreference)) {

                    $state.go(user.defaultUserStartPreference, null, { reload: true });

                } else {

                    $state.go("index", null, { reload: true });

                }
            });
        };

        // when changing states, we might need to authorize the user
        $rootScope.$on("$stateChangeStart", (event, toState: Kitos.AuthRoles, toParams, fromState, fromParams) => {

            if (toState.noAuth) { // no need to auth
                return;
            }

            userService.auth(toState.authRoles).then(val => {
                // authentication OK!
            }, () => {
                event.preventDefault();

                // bad authentication
                $state.go("index", { to: toState.name, toParams: toParams });
            });
        });

        $rootScope.$on("$stateChangeSuccess",
            function (event, toState, toParams, fromState, fromParams) {
                //Check if state comes from another state with same parent - if so, it shall not hide the subnav while changing
                if (toState.name.split(".")[0] != fromState.name.split(".")[0])
                    $rootScope.subnavNotPositioned = true;
            });

        // when something goes wrong during state change (e.g a rejected resolve)
        $rootScope.$on("$stateChangeError", (event, toState, toParams, fromState, fromParams, error) => {
            $state.go("index");
        });

        // Fixes the blank spaces problem when deselecting columns (OS2KITOS-607)
        // When implemented here fixWidthOnClick is shared by IT Project, IT System and IT Contract
        needsWidthFixService.fixWidthOnClick();
    }
]);
