(function(ng, app) {
    
    app.controller("RootCtrl", function ($rootScope, $http, $location) {
        $rootScope.page = {
            title: "Index",
            subnav: []
        };

        $rootScope.user = {};

        //logout function for top navigation bar
        $rootScope.logout = function () {
            $http.post("api/authorize?logout").success(function (result) {
                $rootScope.user = {};
                $location.path("/home");
            });
        };

    });
    
})(angular, App);