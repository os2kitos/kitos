((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-project.edit.references.edit", {
            url: "/editReference/:refId/:orgId",
            onEnter: ["$state", "$stateParams", "$uibModal", "referenceServiceFactory",
                ($state, $stateParams, $modal, referenceServiceFactory) => {
                    var referenceService = referenceServiceFactory.createProjectReference();
                    $modal.open({
                        templateUrl: "app/components/it-reference/it-reference-modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        controller: "project.referenceEditModalCtrl",
                        resolve: {
                            referenceService: [() => referenceService],
                            reference: [() => referenceService.getReference($stateParams.refId)
                                .then(result => result)
                            ]
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

    app.controller("project.referenceEditModalCtrl",
        ["$scope", "reference", "$stateParams", "notify", "referenceService",
            ($scope, reference, $stateParams, notify, referenceService) => {

                $scope.reference = reference;

                $scope.dismiss = () => {
                    $scope.$dismiss();
                };

                $scope.save = () => {
                    var msg = notify.addInfoMessage("Gemmer række", false);

                    referenceService.updateReference(
                        $stateParams.refId,
                        $stateParams.orgId,
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