(function (ng, app) {
    app.config([
        "$stateProvider", function ($stateProvider) {
            $stateProvider.state("forgot-password", {
                url: "/forgot-password",
                templateUrl: "app/components/home/home-forgot-password.view.html",
                controller: "home.ForgotPasswordCtrl",
                noAuth: true
            });
        }
    ]);

    app.controller("home.ForgotPasswordCtrl", [
        "$rootScope", "$scope", "$http", "notify", function ($rootScope, $scope, $http, notify) {
            $rootScope.page.title = "Glemt password";
            $rootScope.page.subnav = [];

            // submit
            $scope.submit = function () {
                if ($scope.requestForm.$invalid) return;

                var email = $scope.email;
                var data = { "email": email };

                var msg = notify.addInfoMessage("Sender email ...", false);

                $http.post("api/passwordresetrequest", data, { handleBusy: true })
                    .then(function onSuccess(result) {
                        msg.toSuccessMessage("En email er blevet sent til " + email);
                        $scope.email = "";

                    }, function onError(result) {
                        msg.toErrorMessage("Emailen kunne ikke sendes. Prøv igen eller kontakt en lokal administrator");
                    });
            };
        }
    ]);
})(angular, app);
