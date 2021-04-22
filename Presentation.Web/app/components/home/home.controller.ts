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

    app.controller("home.IndexCtrl", ["$rootScope", "$scope", "$http", "$state", "$stateParams", "notify", "userService", "texts", "navigationService", "$sce", "$location", "$",
        ($rootScope, $scope, $http, $state, $stateParams, notify, userService, texts, navigationService, $sce, $location, $) => {

            const factory = new Kitos.Models.ViewModel.Sso.SsoStateViewModelFactory($);
            let ssoStateViewModel = factory.createFromViewState();

            if (ssoStateViewModel.startPreference && ssoStateViewModel.startPreference !== "index") {
                if (navigationService.checkState(ssoStateViewModel.startPreference)) {

                    $state.go(ssoStateViewModel.startPreference, null, { reload: true });
                    return;
                }
            }

            $rootScope.page.title = "Index";
            $rootScope.page.subnav = [];
            $scope.texts = [];
            $scope.texts.about = _.find(texts, (textObj: { id; value; }) => (textObj.id == 1));
            $scope.texts.status = _.find(texts, (textObj: { id; value; }) => (textObj.id == 2));
            $scope.texts.guide = _.find(texts, (textObj: { id; value; }) => (textObj.id == 3));
            $scope.texts.support = _.find(texts, (textObj: { id; value; }) => (textObj.id == 4));
            $scope.texts.join = _.find(texts, (textObj: { id; value; }) => (textObj.id == 5));

            /* Fix html to enable alignment etc. */
            $scope.texts.about.value = $sce.trustAsHtml($scope.texts.about.value);
            $scope.texts.status.value = $sce.trustAsHtml($scope.texts.status.value);
            $scope.texts.guide.value = $sce.trustAsHtml($scope.texts.guide.value);
            $scope.texts.support.value = $sce.trustAsHtml($scope.texts.support.value);
            $scope.texts.join.value = $sce.trustAsHtml($scope.texts.join.value);

            $scope.tinymceOptions = {
                plugins: 'link image code',
                skin: 'lightgray',
                theme: 'modern',
                convert_urls: false
            };

            $scope.ssoVm = ssoStateViewModel;

            const resetSsoError = () => {
                $scope.ssoVm.error = null;
            }

            $scope.text = {};

            var token = $location.search()["id_token"];
            $location.search("id_token", null);
            $location.search("scope", null);
            $location.search("session_state", null);

            if (token) {
                userService.loginSSO(token).then(user => $scope.loginResult(user));
            }

            // login
            $scope.submitLogin = () => {
                resetSsoError();

                if ($scope.loginForm.$invalid) return;

                userService.login($scope.email, $scope.password, $scope.remember).then(user => {
                    notify.addSuccessMessage("Du er nu logget ind!");
                    if (user.isAuth === true) {
                        if (navigationService.checkState(user.defaultUserStartPreference)) {
                            $state.go(user.defaultUserStartPreference);
                        } else {
                            $state.go("index");
                        }
                    }
                }, error => {
                    if (error.response === "User is locked")
                        notify.addErrorMessage("Brugeren er låst! Kontakt administrator.");
                    else if (typeof error.response === "undefined") {
                        // closes choose-organization modal without displaying a notification to the user if he/she doesn't belong to any organization
                    }
                    else
                        notify.addErrorMessage("Forkert brugernavn eller password!");
                });
            };

            $scope.save = (text) => {
                var msg = notify.addInfoMessage("Gemmer...", false);

                var payload = { value: text.value };

                $http({ method: 'PATCH', url: 'api/text/' + text.id + '?organizationId=' + 1, data: payload, ignoreLoadingBar: true })
                    .then(function onSuccess(result) {
                        msg.toSuccessMessage("Feltet er opdateret.");
                    }, function onError(result) {
                        if (result.status === 409) {
                            msg.toErrorMessage("Fejl! Feltet kunne ikke ændres da værdien allerede findes i KITOS!");
                        } else {
                            msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                        }
                    });
            }
        }
    ]);
})(angular, app);
