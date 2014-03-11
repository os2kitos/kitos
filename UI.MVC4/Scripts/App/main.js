var App = angular.module("App", ["ngRoute"]);

App.config(["$routeProvider", function ($routeProvider) {
    $routeProvider.when("/home", {
        templateUrl: "partials/home/index.html",
        controller: "home.HomeController"
    }).otherwise({
        redirectTo: "/home"
    });
}]);