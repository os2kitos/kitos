(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.usage.references.create', {
            url: '/createReference/:id',
            onEnter: ['$state', '$stateParams', '$uibModal', 'user',
                function ($state, $stateParams, $modal, user) {
                    $modal.open({
                        templateUrl: "app/components/it-reference/it-reference-modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        controller: "it-system-usage.referenceCreateModalCtrl",
                        resolve: {
                            itSystemUsage: [
                                '$http', '$stateParams', function ($http, $stateParams) {
                                    return $http.get('api/itSystemUsage/' + $stateParams.id)
                                        .then(function (result) {
                                            return result.data.response;
                                        });
                                }
                            ],
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

    app.controller("it-system-usage.referenceCreateModalCtrl",
        ["$scope", "$http","itSystemUsage","notify","user",
            function ($scope, $http, itSystemUsage,notify,user) {

                $scope.dismiss = function () {
                    $scope.$dismiss();
                };

                $scope.save = function () {

                    var created = new Date();

                    var data = {
                        ItSystemUsage_Id: itSystemUsage.id,
                        Title: $scope.reference.title,
                        ExternalReferenceId: $scope.reference.externalReferenceId,
                        URL: $scope.reference.url,
                        Display: $scope.reference.display,
                        Created: created
                    };

                    var msg = notify.addInfoMessage("Gemmer række", false);
                    $http.post("api/Reference", data)
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
