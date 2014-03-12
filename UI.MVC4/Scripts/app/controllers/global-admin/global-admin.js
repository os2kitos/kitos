(function (ng, app) {

    app.config(["$routeProvider", function ($routeProvider) {
        $routeProvider.when("/global-admin/new-municipality", {
            templateUrl: "partials/global-admin/new-municipality.html",
            controller: "globalAdmin.NewMunicipalityCtrl"
        }).when("/global-admin", {
            redirectTo: "/global-admin/new-municipality"
        });
    }]);

    app.controller("globalAdmin.NewMunicipalityCtrl", function ($rootScope, $scope) {
        $rootScope.page.title = "Ny kommune";
        $rootScope.page.subnav = [];
    });

})(angular, App);