(function (ng, app) {
    app.config([
        "$stateProvider", function ($stateProvider) {
            $stateProvider.state("reset-password", {
                url: "/reset-password/:requestId",
                templateUrl: "app/components/home/home-reset-password.view.html",
                controller: "home.ResetPasswordCtrl",
                noAuth: true
            });
        }
    ]);

    app.controller("home.ResetPasswordCtrl", [
        "$rootScope", "$scope", "$http", "$stateParams", function ($rootScope, $scope, $http, $stateParams) {
            $rootScope.page.title = "Nyt password";
            $rootScope.page.subnav = [];

            var requestId = $stateParams.requestId;
            $http.get("api/passwordresetrequest?requestId=" + requestId)
                .then(function onSuccess(result) {
                    $scope.resetStatus = "enterPassword";
                    $scope.email = result.data.response.userEmail;
                }, function onError(result) {
                    $scope.resetStatus = "missingRequest";
                });

            $scope.submit = function () {
                if ($scope.resetForm.$invalid) return;

                var data = { "requestId": requestId, "newPassword": $scope.password };

                $http.post("api/authorize?resetPassword", data)
                    .then(function onSuccess(result) {
                        $scope.resetStatus = "success";
                        $scope.email = "";
                    }, function onError(result) {
                        $scope.requestFailure = true;
                    });
            };
        }
    ]);
})(angular, app);
