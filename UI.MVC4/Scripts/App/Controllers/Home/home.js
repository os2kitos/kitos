(function(ng, app) {

    app.controller("home.IndexController", function ($rootScope, $scope) {
        $rootScope.page = { title: "Index" };
    });
    
    app.controller("home.LoginController", function ($rootScope, $scope) {
        $rootScope.page = { title: "Log ind" };
        //$rootScope.subnav = [{ text: "test" }, { text: "foo" }];
    });

})(angular, App);