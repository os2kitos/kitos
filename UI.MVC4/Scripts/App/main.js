var App = angular.module("App", ["ui.router"]);

App.config(function ($urlRouterProvider) {
    $urlRouterProvider.otherwise("/");
});

App.run(function ($rootScope, $http, $state) {
    $rootScope.page = {
        title: "Index",
        subnav: []
    };

    $rootScope.user = {};

    //logout function for top navigation bar
    $rootScope.logout = function () {
        $http.post("api/authorize?logout").success(function (result) {
            $rootScope.user = {};
            $state.go("index");
        });
    };

    $rootScope.saveUser = function (result) {
        console.log("Saving user: " + result);

        $rootScope.user = {
            name: result.Response.Name,
            email: result.Response.Email,
            municipality: result.Response.Municipality_Id,
            authStatus: "authorized",
            role: result.Response.RoleName
        };
    };

    var startPromise = $http.get("api/authorize").success($rootScope.saveUser);

    $rootScope.$on("$stateChangeStart", function (event, toState, toParams, fromState, fromParams) {
        if (toState.noAuth) return; //no need to auth

        

        var user = $rootScope.user;
        var userRole = user.role;
        var authRoles = toState.authRoles;
        
        if (user.authStatus != "authorized" || (authRoles && _.indexOf(authRoles, userRole) == -1)) {
            $state.transitionTo("login");
            event.preventDefault();
        }

    });
});