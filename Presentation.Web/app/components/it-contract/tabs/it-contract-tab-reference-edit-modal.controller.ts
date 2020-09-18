//((ng, app) => {
//    app.config(["$stateProvider", $stateProvider => {
//        $stateProvider.state("it-contract.edit.references.edit", {
//            url: "/editReference/:refId/:orgId",
//            onEnter: ["$state", "$stateParams", "$uibModal", "referenceServiceFactory",
//                ($state, $stateParams, $modal, referenceServiceFactory) => {
//                    var referenceService = referenceServiceFactory.createContractReference();
//                    $modal.open({
//                        templateUrl: "app/components/it-reference/it-reference-modal.view.html",
//                        // fade in instead of slide from top, fixes strange cursor placement in IE
//                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
//                        windowClass: "modal fade in",
//                        controller: "contract.referenceEditModalCtrl",
//                        resolve: {
//                            referenceService: [() => referenceService],
//                            reference: [() => referenceService.getReference($stateParams.refId)
//                                .then(result => result)
//                            ]
//                        }
//                    }).result.then(() => {
//                        // OK
//                        // GOTO parent state and reload
//                        $state.go("^", null, { reload: true });
//                    }, () => {
//                        // Cancel
//                        // GOTO parent state
//                        $state.go("^");
//                    });
//                }
//            ]
//        });
//    }]);

//    app.controller("contract.referenceEditModalCtrl",
//        ["$scope", "reference", "$stateParams", "notify", "referenceService",
//            ($scope, reference, $stateParams, notify, referenceService) => {
//                $scope.reference = reference;

//                $scope.dismiss = () => {
//                    $scope.$dismiss();
//                };

//                $scope.save = () => {
//                    var msg = notify.addInfoMessage("Gemmer række", false);

//                    referenceService.updateReference(
//                        $stateParams.refId,
//                        $stateParams.orgId,
//                        $scope.reference.title,
//                        $scope.reference.externalReferenceId,
//                        $scope.reference.url)
//                        .then(success => {
//                            msg.toSuccessMessage("Referencen er gemt");
//                            $scope.$close(true);
//                        },
//                            error => msg.toErrorMessage("Fejl! Prøv igen"));
//                };
//            }]);
//})(angular, app);


module Kitos.Contract.Edit.Reference.Edit {
    "use strict";

    export class EditReferenceItContractEditController {
        static $inject: Array<string> = [
            "$scope",
            "notify",
            "$state",
            "$stateParams",
            "$uibModalInstance",
            "referenceService",
            "reference"

        ];

        constructor(
            private readonly $scope,
            private readonly notify,
            private readonly $state: angular.ui.IStateService,
            private readonly $stateParams,
            private readonly $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private referenceService: Services.ReferenceService,
            private reference) {

            $scope.reference = this.reference;
            $scope.modalTitle = "Rediger reference";
        }

        save() {

            var msg = this.notify.addInfoMessage("Gemmer række", false);

            this.referenceService.updateReference(
                this.$stateParams.refId,
                this.$stateParams.orgId,
                this.$scope.reference.title,
                this.$scope.reference.externalReferenceId,
                this.$scope.reference.url)
                .then(success => {
                    msg.toSuccessMessage("Ændringer er gemt");
                    this.close();
                    this.popState(true);
                },
                    error => msg.toErrorMessage("Fejl! Prøv igen"));
        }

        private close() {
            this.$uibModalInstance.close();
        }

        cancel(): void {
            this.close();
            this.popState();
        }

        private popState(reload = false) {
            const popped = this.$state.go("^");
            if (reload) {
                popped.then(() => this.$state.reload());
            }
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("it-contract.edit.references.edit", {
                url: "/editReference/:refId/:orgId",
                onEnter: [
                    "$state", "$stateParams", "$uibModal", "referenceServiceFactory",
                    ($state: ng.ui.IStateService, $stateParams, $uibModal: ng.ui.bootstrap.IModalService, referenceServiceFactory) => {
                        var referenceService = referenceServiceFactory.createContractReference();
                        $uibModal.open({
                            templateUrl: "app/components/it-reference/it-reference-modal.view.html",
                            windowClass: "modal fade in",
                            resolve: {
                                referenceService: ["referenceServiceFactory", (referenceServiceFactory) => referenceServiceFactory.createContractReference()],
                                reference: [() => referenceService.getReference($stateParams.refId).then(result => result)]
                            },
                            controller: EditReferenceItContractEditController,
                            controllerAs: "vm",
                        }).result.then(() => {

                        },
                            () => {
                                $state.go("^");
                            });
                    }
                ]
            });
        }]);
}

