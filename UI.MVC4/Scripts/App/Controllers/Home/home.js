(function(ng, app) {

    app.controller("RootCtrl", function ($rootScope, $http, $location) {
        $rootScope.page = {
            title: "Index",
            subnav: []
        };

        $rootScope.user = {};

        //logout function for top navigation bar
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
        
        //login
        $scope.submit = function () {
            if ($scope.loginForm.$invalid) return;

            var data = {
                "Email": $scope.email,
                "Password": $scope.password,
                "RememberMe": $scope.remember
            };

            $http.post("api/authorize", data).success(function(result) {
                $rootScope.user = {
                    name: result.Response.Name,
                    email: result.Response.Email,
                    municipality: result.Response.Municipality_Id,
                    authStatus: "authorized"
                };
                $location.path("/home");
            });

        };
    });

    app.controller("home.ForgotPasswordCtrl", function ($rootScope, $scope, $http) {
        $rootScope.page.title = "Glemt password";
        
        //submit 
        $scope.submit = function() {
            if ($scope.requestForm.$invalid) return;

            var data = { "Email": $scope.email };

            $scope.requestSuccess = $scope.requestFailure = false;
            
            $http.post("api/passwordresetrequest", data).success(function(result) {
                $scope.requestSuccess = true;
                $scope.email = "";
            }).error(function (result) {
                $scope.requestFailure = true;
            });;
        };
    });

    app.controller("home.ResetPasswordCtrl", function ($rootScope, $scope, $http) {
        $rootScope.page.title = "Nyt password";
    });

})(angular, App);