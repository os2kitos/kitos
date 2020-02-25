((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-contract.edit.references.create", {
            url: "/createReference/:id",
            onEnter: ["$state", "$uibModal",
                ($state, $modal) => {
                    $modal.open({
                        templateUrl: "app/components/it-reference/it-reference-modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        controller: "contract.referenceCreateModalCtrl",
                        resolve: {
                            referenceService: ["referenceServiceFactory", (referenceServiceFactory) => referenceServiceFactory.createContractReference()],
                        }

                    }).result.then(() => {
                        // OK
                        // GOTO parent state and reload
                        $state.go("^", null, { reload: true });
                    }, () => {
                        // Cancel
                        // GOTO parent state
                        $state.go("^");
                    });
                }
            ]
        });
    }]);

    app.controller("contract.referenceCreateModalCtrl",
        ["$scope", "notify", "referenceService", "$stateParams",
            ($scope, notify, referenceService, $stateParams) => {

                $scope.dismiss = () => {
                    $scope.$dismiss();
                };

                $scope.save = () => {

                    var msg = notify.addInfoMessage("Gemmer række", false);
                    referenceService.createReference(
                            $stateParams.id,
                            $scope.reference.title,
                            $scope.reference.externalReferenceId,
                            $scope.reference.url)
                        .then(success => {
                                msg.toSuccessMessage("Referencen er gemt");
                                $scope.$close(true);
                            },
                            error => msg.toErrorMessage("Fejl! Prøv igen"));

                };
            }]);
})(angular, app);
