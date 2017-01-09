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
    "oidc-angular"]);

app.constant("JSONfn", JSONfn)
    .constant("moment", moment)
    .constant("$", $)
    .constant("_", _);

app.config([
    "$urlRouterProvider", ($urlRouterProvider: angular.ui.IUrlRouterProvider) => {
        $urlRouterProvider.otherwise("/");
    }
]);

app.config(['$authProvider', $authProvider => {
    $authProvider.configure({
        redirectUri: location.origin + "/#/?",
        scope: "openid email",
        //TODO: Should be fetched from backend (SSOGateway in web.config)
        basePath: 'https://os2sso-test.miracle.dk',
        //TODO: Should be fetched from backend (SSOAudience in web.config)
        clientId: 'kitos_client'
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
    "$rootScope", "$http", "$state", "$uibModal", "notify", "userService", "uiSelect2Config", "navigationService","$auth",
    ($rootScope, $http, $state, $modal, notify, userService, uiSelect2Config, navigationService, $auth) => {

        $rootScope.$on('oidcauth:loggedIn', function (e) {
            console.log('[EventCallback]', 'Event', e.name, e);
            console.log('[EventCallback]', '$auth.isAuthenticated', $auth.isAuthenticated());
        });

        // init info
        $rootScope.page = {
            title: "Index",
            subnav: []
        };

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

        // when something goes wrong during state change (e.g a rejected resolve)
        $rootScope.$on("$stateChangeError", (event, toState, toParams, fromState, fromParams, error) => {
            console.log(error);
            $state.go("index");
        });

        

    }
]);
