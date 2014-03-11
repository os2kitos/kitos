(function(ng, app) {

    app.controller("home.HomeController", function ($rootScope, $scope) {
        $rootScope.page = { title: "Index" };
        $rootScope.subnav = [{ text: "test" }, { text: "foo" }];
    });

})(angular, App);