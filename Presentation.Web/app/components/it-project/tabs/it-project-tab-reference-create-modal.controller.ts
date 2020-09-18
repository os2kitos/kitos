module Kitos.ItProject.Edit.Reference.Create {
    "use strict";

    export class CreateReferenceItProjectEditController {
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
            $stateProvider.state("it-project.edit.references.create", {
                url: "/createReference/:id",
                onEnter: [
                    "$state", "$uibModal",
                    ($state: ng.ui.IStateService, $uibModal: ng.ui.bootstrap.IModalService) => {
                        $uibModal.open({
                            templateUrl: "app/components/it-reference/it-reference-modal.view.html",
                            windowClass: "modal fade in",
                            resolve: {
                                referenceService: ["referenceServiceFactory", (referenceServiceFactory) => referenceServiceFactory.createProjectReference()],
                            },
                            controller: CreateReferenceItProjectEditController,
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

