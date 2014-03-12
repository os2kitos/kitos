(function(ng, app) {

    app.config(["$routeProvider", function ($routeProvider) {
        $routeProvider.when("/home", {
            templateUrl: "partials/home/index.html",
            controller: "home.IndexCtrl"
        }).when("/home/login", {
            templateUrl: "partials/home/login.html",
            controller: "home.LoginCtrl"
        }).when("/home/forgot-password", {
            templateUrl: "partials/home/forgot-password.html",
            controller: "home.ForgotPasswordCtrl"
        }).when("/home/reset-password/:requestId", {
            templateUrl: "partials/home/reset-password.html",
            controller: "home.ResetPasswordCtrl"
        }).otherwise({
            redirectTo: "/home"
        });
    }]);

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

                console.log(result);
            }).error(function (result) {
                $scope.requestFailure = true;
            });
        };
    });

    app.controller("home.ResetPasswordCtrl", function ($rootScope, $scope, $http, $routeParams) {
        $rootScope.page.title = "Nyt password";
        
        function noRequest() {
            $scope.resetStatus = "noRequest";
        }

        var requestId = $routeParams.requestId;
        $http.get("api/passwordresetrequest?requestId=" + requestId).success(function(result) {
            $scope.resetStatus = "enterPassword";
            $scope.email = result.Response.UserEmail;
        }).error(function() {
            $scope.resetStatus = "missingRequest";
        });

        $scope.submit = function() {
            if ($scope.resetForm.$invalid) return;

            var data = { "RequestId": requestId, "NewPassword": $scope.password };
            
            $http.post("api/authorize?resetPassword", data).success(function (result) {
                $scope.resetStatus = "success";
                $scope.email = "";
            }).error(function (result) {
                $scope.requestFailure = true;
            });
        };


    });

})(angular, App);