var App = angular.module("App", ["ngRoute"]);

App.config(["$routeProvider", function ($routeProvider) {
    $routeProvider.when("/home", {
        templateUrl: "partials/home/index.html",
        controller: "home.IndexCtrl"
    }).when("/home/login", {
        templateUrl: "partials/home/login.html",
        controller: "home.LoginCtrl"
    }).when("/home/forgot-password", {
        templateUrl: "partials/home/forgot-password.html",
        controller: "home.ForgotPasswordCtrl"
    }).otherwise({
        redirectTo: "/home"
    });
}]);