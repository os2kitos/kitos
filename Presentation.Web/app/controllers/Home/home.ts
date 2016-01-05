(function(ng, app) {
    app.config([
        '$stateProvider', function($stateProvider) {
            $stateProvider.state('index', {
                url: '/?to',
                templateUrl: 'partials/home/index.html',
                controller: 'home.IndexCtrl',
                noAuth: true,
                resolve: {
                    texts: [
                        '$http', function($http) {
                            return $http.get("api/text/")
                                .then(function(result) {
                                    return result.data.response;
                                });
                        }
                    ]
                }
            }).state('forgot-password', {
                url: '/forgot-password',
                templateUrl: 'partials/home/forgot-password.html',
                controller: 'home.ForgotPasswordCtrl',
                noAuth: true
            }).state('reset-password', {
                url: '/reset-password/:requestId',
                templateUrl: 'partials/home/reset-password.html',
                controller: 'home.ResetPasswordCtrl',
                noAuth: true
            });
        }
    ]);

    app.controller('home.IndexCtrl', [
        '$rootScope', '$scope', '$http', '$state', '$stateParams', 'notify', 'userService', 'texts',
        function($rootScope, $scope, $http, $state, $stateParams, notify, userService, texts) {
            $rootScope.page.title = 'Index';
            $rootScope.page.subnav = [];

            $scope.about = _.find(texts, function(textObj) {
                return textObj.id == 1;
            }).value;

            $scope.status = _.find(texts, function(textObj) {
                return textObj.id == 2;
            }).value;

            //login
            $scope.submitLogin = function() {
                if ($scope.loginForm.$invalid) return;

                userService.login($scope.email, $scope.password, $scope.remember)
                    .then(function() {
                        notify.addSuccessMessage("Du er nu logget ind!");
                    }, function (error) {
                        if (error.response === "User is locked")
                            notify.addErrorMessage("Brugeren er låst! Kontakt administrator.");
                        else
                            notify.addErrorMessage("Forkert brugernavn eller password!");
                    });
            };
        }
    ]);

    app.controller('home.ForgotPasswordCtrl', [
        '$rootScope', '$scope', '$http', 'notify', function($rootScope, $scope, $http, notify) {
            $rootScope.page.title = 'Glemt password';
            $rootScope.page.subnav = [];

            //submit 
            $scope.submit = function() {
                if ($scope.requestForm.$invalid) return;

                var email = $scope.email;
                var data = { "email": email };

                var msg = notify.addInfoMessage("Sender email ...", false);

                $http.post('api/passwordresetrequest', data, { handleBusy: true }).success(function(result) {
                    msg.toSuccessMessage("En email er blevet sent til " + email);
                    $scope.email = '';

                }).error(function(result) {
                    msg.toErrorMessage("Emailen kunne ikke sendes. Prøv igen eller kontakt en lokal administrator");
                });
            };
        }
    ]);

    app.controller('home.ResetPasswordCtrl', [
        '$rootScope', '$scope', '$http', '$stateParams', function($rootScope, $scope, $http, $stateParams) {
            $rootScope.page.title = 'Nyt password';
            $rootScope.page.subnav = [];

            var requestId = $stateParams.requestId;
            $http.get('api/passwordresetrequest?requestId=' + requestId).success(function(result) {
                $scope.resetStatus = 'enterPassword';
                $scope.email = result.response.userEmail;
            }).error(function() {
                $scope.resetStatus = 'missingRequest';
            });

            $scope.submit = function() {
                if ($scope.resetForm.$invalid) return;

                var data = { "requestId": requestId, "newPassword": $scope.password };

                $http.post('api/authorize?resetPassword', data).success(function(result) {
                    $scope.resetStatus = 'success';
                    $scope.email = '';
                }).error(function(result) {
                    $scope.requestFailure = true;
                });
            };
        }
    ]);
})(angular, app);
