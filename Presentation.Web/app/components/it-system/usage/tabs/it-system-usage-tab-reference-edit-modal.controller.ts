(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.usage.references.edit', {
            url: '/editReference/:refId/:orgId',
            onEnter: ['$state', '$stateParams', '$uibModal','$http',
                function ($state, $stateParams, $modal, $http) {
                    $modal.open({
                        templateUrl: "app/components/it-reference-modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        controller: "it-system-usage.referenceEditModalCtrl",
                       resolve: {
                           reference: ["$http", function ($http) {

                                return $http.get("api/Reference/" + $stateParams.refId)
                                    .then(function (result) {
                                        return result.data.response;
                                   });
                           }],
                            user: [
                                'userService', function (userService) {
                                    return userService.getUser().then(function (user) {
                                        return user;
                                    });
                                }
                            ]
                        }
                    }).result.then(function () {
                        // OK
                        // GOTO parent state and reload
                        $state.go("^", null, { reload: true });
                    }, function () {
                        // Cancel
                        // GOTO parent state
                        $state.go("^");
                    });
                }
            ]
        });
    }]);

    app.controller("it-system-usage.referenceEditModalCtrl",
        ["$scope", "$http", "reference","$stateParams","notify",
            function ($scope, $http, reference, $stateParams,notify) {

                $scope.reference = reference;

                $scope.dismiss = function () {
                    $scope.$dismiss();
                };

                    $scope.save = function () {
                        
                        var data = {
                            Title: $scope.reference.title,
                            ExternalReferenceId: $scope.reference.externalReferenceId,
                            URL: $scope.reference.url,
                            Display: $scope.reference.display
                        };

                        var msg = notify.addInfoMessage("Gemmer række", false);

                        $http.patch("api/Reference/" + $stateParams.refId + "?organizationId=" + $stateParams.orgId, data)
                            .success(function (result) {
                                msg.toSuccessMessage("Referencen er gemt");
                                $scope.$close(true);
                            })
                            .error(function () {
                                msg.toErrorMessage("Fejl! Prøv igen");
                            });
                };
            }]);
})(angular, app);