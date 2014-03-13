(function (ng, app) {
   
    
    app.config(["$stateProvider", "$urlRouterProvider", function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state("global-admin", {
            url: "/global-admin",
            templateUrl: "partials/global-admin/new-municipality.html",
            controller: "globalAdmin.NewMunicipalityCtrl",
            authRoles: ["GlobalAdmin"]
        });

    }]);
    
    app.controller("globalAdmin.NewMunicipalityCtrl", function ($rootScope, $scope) {
        $rootScope.page.title = "Ny kommune";
        $rootScope.page.subnav = [];
    });

})(angular, App);