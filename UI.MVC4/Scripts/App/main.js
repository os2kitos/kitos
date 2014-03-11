var App = angular.module("App", ["ngRoute"]);

App.config(["$routeProvider", function ($routeProvider) {
    $routeProvider.when("/home", {
        templateUrl: "partials/home/index.html",
        controller: "home.IndexController"
    }).when("/home/login", {
        templateUrl: "partials/home/login.html",
        controller: "home.LoginController"
    }).otherwise({
        redirectTo: "/home"
    });
}]);