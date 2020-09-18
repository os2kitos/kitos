//((ng, app) => {
//    app.config(["$stateProvider", $stateProvider => {
//        $stateProvider.state("it-contract.edit.references.create", {
//            url: "/createReference/:id",
//            onEnter: ["$state", "$uibModal", ($state, $modal) => {
//                $modal.open({
//                    templateUrl: "app/components/it-reference/it-reference-modal.view.html",
//                    // fade in instead of slide from top, fixes strange cursor placement in IE
//                    // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
//                    windowClass: "modal fade in",
//                    controller: "contract.referenceCreateModalCtrl",
//                    resolve: {
//                        referenceService: ["referenceServiceFactory", (referenceServiceFactory) => referenceServiceFactory.createContractReference()],

//                        // Even though they are never used by the controller or any part of this code these are required. Otherwise an injector error is thrown.
//                        contract: ["$http", "$stateParams", ($http, $stateParams) => $http.get(`api/itcontract/${$stateParams.id}`).then(result => result.data.response)],
//                        user: ["userService", userService => userService.getUser().then(user => user)],
//                    }

//                }).result.then(() => {
//                    // OK
//                    // GOTO parent state and reload
//                    $state.go("^", null, { reload: true });
//                }, () => {
//                    // Cancel
//                    // GOTO parent state
//                    $state.go("^");
//                });
//            }
//            ]
//        });
//    }]);

//    app.controller("contract.referenceCreateModalCtrl", ["$scope", "notify", "referenceService", "$stateParams", 
//        ($scope, notify, referenceService, $stateParams) => {

//            $scope.dismiss = () => {
//                $scope.$dismiss();
//            };

//            $scope.save = () => {

//                var msg = notify.addInfoMessage("Gemmer række", false);
//                referenceService.createReference(
//                    $stateParams.id,
//                    $scope.reference.title,
//                    $scope.reference.externalReferenceId,
//                    $scope.reference.url)
//                    .then(success => {
//                        msg.toSuccessMessage("Referencen er gemt");
//                        $scope.$close(true);
//                    },
//                        error => msg.toErrorMessage("Fejl! Prøv igen"));

//            };
//        }]);
//})(angular, app);


module Kitos.Contract.Edit.Reference.Create {
    "use strict";

    export class CreateReferenceItContractEditController {
        static $inject: Array<string> = [
            "$scope",
            "notify",
            "$state",
            "$stateParams",
            "$uibModalInstance",
            "referenceService"

        ];

        constructor(
            private readonly $scope,
            private readonly notify,
            private readonly $state: angular.ui.IStateService,
            private readonly $stateParams,
            private readonly $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private referenceService: Services.ReferenceService) {

            $scope.modalTitle = "Opret reference";
        }

        save() {

            var msg = this.notify.addInfoMessage("Gemmer række", false);

            this.referenceService.createReference(
                this.$stateParams.id,
                this.$scope.reference.title,
                this.$scope.reference.externalReferenceId,
                this.$scope.reference.url).then(success => {
                    msg.toSuccessMessage("Referencen er gemt");
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
            $stateProvider.state("it-contract.edit.references.create", {
                url: "/createReference/:id",
                onEnter: [
                    "$state", "$uibModal",
                    ($state: ng.ui.IStateService, $uibModal: ng.ui.bootstrap.IModalService) => {
                        $uibModal.open({
                            templateUrl: "app/components/it-reference/it-reference-modal.view.html",
                            windowClass: "modal fade in",
                            resolve: {
                                referenceService: ["referenceServiceFactory", (referenceServiceFactory) => referenceServiceFactory.createContractReference()],
                            },
                            controller: CreateReferenceItContractEditController,
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

