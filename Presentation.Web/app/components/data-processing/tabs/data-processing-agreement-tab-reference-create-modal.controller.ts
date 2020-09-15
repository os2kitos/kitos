module Kitos.DataProcessing.Agreement.Edit.Reference.Create {
    "use strict";

    export class CreateReferenceDataProcessingAgreementController {
        static $inject: Array<string> = [
            "dataProcessingAgreementService",
            "user",
            "$scope",
            "notify",
            "$state",
            "$stateParams",
            "$uibModalInstance",
            "referenceService"

        ];

        constructor(
            private readonly dataProcessingAgreementService: Services.DataProcessing.IDataProcessingAgreementService,
            private readonly user: Services.IUser,
            private readonly $scope,
            private readonly notify,
            private readonly $state: angular.ui.IStateService,
            private readonly $stateParams,
            private readonly $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private referenceService: Services.ReferenceService) {
        }

        save() {

            var msg = this.notify.addInfoMessage("Gemmer række", false);

            this.referenceService.createReference(
                this.$stateParams.id,
                this.$scope.reference.title,
                this.$scope.reference.referenceId,
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
            $stateProvider.state("data-processing.edit-agreement.reference.create", {
                url: "/createReference/:id",
                onEnter: [
                    "$state", "$uibModal",
                    ($state: ng.ui.IStateService, $uibModal: ng.ui.bootstrap.IModalService) => {
                        $uibModal.open({
                            templateUrl: "app/components/data-processing/tabs/data-processing-agreement-tab-reference-create-modal.view.html",
                            windowClass: "modal fade in",
                            resolve: {
                                user: ["userService", (userService: Services.IUserService) => userService.getUser()],
                                referenceService: ["referenceServiceFactory", (referenceServiceFactory) => referenceServiceFactory.createDpaReference()],
                            },
                            controller: CreateReferenceDataProcessingAgreementController,
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