module Kitos.DataProcessing.Agreement.Edit.Reference.Edit {
    "use strict";

    export class EditReferenceDataProcessingAgreementController {
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
            $stateProvider.state("data-processing.edit-agreement.reference.edit", {
                url: "/editReference/:refId/:orgId",
                onEnter: [
                    "$state", "$stateParams", "$uibModal", "referenceServiceFactory",
                    ($state: ng.ui.IStateService, $stateParams, $uibModal: ng.ui.bootstrap.IModalService, referenceServiceFactory) => {
                        var referenceService = referenceServiceFactory.createDpaReference();
                        $uibModal.open({
                            templateUrl: "app/components/it-reference/it-reference-modal.view.html",
                            windowClass: "modal fade in",
                            resolve: {
                                referenceService: ["referenceServiceFactory", (referenceServiceFactory) => referenceServiceFactory.createDpaReference()],
                                reference: [() => referenceService.getReference($stateParams.refId).then(result => result)]
                            },
                            controller: EditReferenceDataProcessingAgreementController,
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