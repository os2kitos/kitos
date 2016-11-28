((ng, app) => {
    app.config([
        "$stateProvider", $stateProvider => {
            $stateProvider.state("index", {
                url: "/?to",
                templateUrl: "app/components/home/home.view.html",
                controller: "home.IndexCtrl",
                noAuth: true,
                resolve: {
                    texts: [
                        "$http", $http => $http.get("api/text/")
                        .then(result => result.data.response)
                    ]
                }
            });
        }
    ]);

    app.controller("home.IndexCtrl", [
        "$rootScope", "$scope", "$http", "$state", "$stateParams", "notify", "userService", "texts", 
        ($rootScope, $scope, $http, $state, $stateParams, notify, userService, texts) => {
            $rootScope.page.title = "Index";
            $rootScope.page.subnav = [];

            $scope.about = _.find(texts, (textObj: { id; value; }) => (textObj.id == 1)).value;

            $scope.status = _.find(texts, (textObj: { id; value; }) => (textObj.id == 2)).value;

            // login
            $scope.submitLogin = () => {
                if ($scope.loginForm.$invalid) return;

                userService.login($scope.email, $scope.password, $scope.remember)
                    .then(() => {
                        notify.addSuccessMessage("Du er nu logget ind!");
                        userService.getUser()
                            .then(data => {
                                if (data.isAuth === true) {
                                    if (data.defaultUserStartPreference == null) {
                                        $state.go("index");
                                    } else {
                                        $state.go(data.defaultUserStartPreference);
                                    }
                                };
                            });
                    }, error => {
                        if (error.response === "User is locked")
                            notify.addErrorMessage("Brugeren er låst! Kontakt administrator.");
                        else
                            notify.addErrorMessage("Forkert brugernavn eller password!");
                    });
            };
        }
    ]);
})(angular, app);
