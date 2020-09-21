module Kitos.Shared.GenericTabs.Reference {
    "use strict";

    export class EditModalReferenceController {
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
            private referenceService: Services.IReferenceService,
            private reference: Models.Reference.IOdataReference) {

            $scope.reference = this.reference;
            $scope.modalTitle = "Rediger reference";
        }

        save() {

            var msg = this.notify.addInfoMessage("Gemmer række", false);
            var referenceScope = this.$scope.reference;

            this.referenceService.updateReference(
                this.$stateParams.refId,
                this.$stateParams.orgId,
                referenceScope.title,
                referenceScope.externalReferenceId,
                referenceScope.url)
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
}