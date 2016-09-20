var app = angular.module("app", [
    "ui.router",
    "ui.bootstrap",
    "ui.select",
    "ngAnimate",
    "notify",
    "angularjs-dropdown-multiselect",
    "ngSanitize",
    "kendo.directives",
    "angular-loading-bar"]);

app.constant("JSONfn", JSONfn)
    .constant("moment", moment)
    .constant("$", $)
    .constant("_", _);

app.config([
    "$urlRouterProvider", ($urlRouterProvider: angular.ui.IUrlRouterProvider) => {
        $urlRouterProvider.otherwise("/");
    }
]);

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
        notifyProvider.globalTimeToLive(5000);
        notifyProvider.onlyUniqueMessages(false);

        // $window isn't ready yet, so fetch it ourself
        var $window = $windowProvider.$get();

        // encode all url requests - fixes IE not correctly encoding special chars
        $httpProvider.interceptors.push(() => ({
            request(config) {
                config.url = $window.encodeURI(config.url);
                return config;
            }
        }));
    }
]);

app.run([
    "$rootScope", "$http", "$state", "$uibModal", "notify", "userService", "uiSelectConfig",
    ($rootScope, $http, $state, $modal, notify, userService, uiSelectConfig) => {
        // init info
        $rootScope.page = {
            title: "Index",
            subnav: []
        };

        $rootScope.$state = $state;

        // this will try to authenticate - to see if the user's already logged in
        userService.getUser();

        uiSelectConfig.dropdownAutoWidth = true;

        // logout function for top navigation bar
        $rootScope.logout = () => {
            userService.logout().then(() => {
                $state.go("index");
            });
        };

        // when changing states, we might need to authorize the user
        $rootScope.$on("$stateChangeStart", (event, toState, toParams, fromState, fromParams) => {

            if (toState.noAuth) { // no need to auth
                return;
            }

            userService.auth(toState.adminRoles).then(val => {
                // authentication OK!

            }, () => {
                event.preventDefault();

                // bad authentication
                $state.go("index", { to: toState.name, toParams: toParams });
            });
        });

        // when something goes wrong during state change (e.g a rejected resolve)
        $rootScope.$on("$stateChangeError", (event, toState, toParams, fromState, fromParams, error) => {
            console.log(error);
            $state.go("index");
        });
    }
]);
