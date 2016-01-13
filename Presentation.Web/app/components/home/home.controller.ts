(function(ng, app) {
    app.config([
        "$stateProvider", function($stateProvider) {
            $stateProvider.state("index", {
                url: "/?to",
                templateUrl: "app/components/home/home.view.html",
                controller: "home.IndexCtrl",
                noAuth: true,
                resolve: {
                    texts: [
                        "$http", function($http) {
                            return $http.get("api/text/")
                                .then(function(result) {
                                    return result.data.response;
                                });
                        }
                    ]
                }
            });
        }
    ]);

    app.controller("home.IndexCtrl", [
        "$rootScope", "$scope", "$http", "$state", "$stateParams", "notify", "userService", "texts",
        function($rootScope, $scope, $http, $state, $stateParams, notify, userService, texts) {
            $rootScope.page.title = "Index";
            $rootScope.page.subnav = [];

            $scope.about = _.find(texts, function (textObj: { id; value; }) {
                return textObj.id == 1;
            }).value;

            $scope.status = _.find(texts, function (textObj: { id; value; }) {
                return textObj.id == 2;
            }).value;

            // login
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
})(angular, app);
