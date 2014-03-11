(function(ng, app) {

    app.controller("RootCtrl", function ($rootScope, $http, $location) {
        $rootScope.page = {
            title: "",
            subnav: []
        };

        $rootScope.user = {};

        $rootScope.logout = function() {
            $http.post("api/authorize?logout").success(function(result) {
                $rootScope.user = {};
                $location.path("/home");
            });
        };

    });

    app.controller("home.IndexCtrl", function ($rootScope, $scope) {
        $rootScope.page.title = "Index";
    });
    
    app.controller("home.LoginCtrl", function ($rootScope, $scope, $http, $location) {
        $rootScope.page.title = "Log ind";
        
        $scope.login = function() {
            if ($scope.loginForm.$invalid) {
                return;
            }

            var data = {
                "Email": $scope.email,
                "Password": $scope.password,
                "RememberMe": $scope.remember
            };

            $http.post("api/authorize", data).success(function(result) {
                $rootScope.user = {
                    name: result.response.Name,
                    email: result.response.Email,
                    municipality: result.response.Municipality_Id,
                    authStatus: "authorized"
                };
                $location.path("/home");
            });

        }
        //$rootScope.subnav = [{ text: "test" }, { text: "foo" }];
    });

    app.controller("home.ForgotPasswordCtrl", function($scope) {
        $scope.page = { title: "Glemt password" };
    });

})(angular, App);