module Kitos.Shared.GenericTabs.Reference {
    "use strict";

    export class CreateModalReferenceController {
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
            private readonly referenceService: Services.IReferenceService) {

            $scope.modalTitle = "Opret reference";
        }

        save() {

            var msg = this.notify.addInfoMessage("Gemmer række", false);
            var referenceScope = this.$scope.reference;

            this.referenceService.createReference(
                this.$stateParams.id,
                referenceScope.title,
                referenceScope.externalReferenceId,
                referenceScope.url).then(success => {
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
}