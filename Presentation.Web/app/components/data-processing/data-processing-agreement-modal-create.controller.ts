module Kitos.DataProcessing.Agreement.Create {
    "use strict";

    export class CreateDateProcessingAgreementController {
        static $inject: Array<string> = [
            "dataProcessingAgreementService",
            "user",
            "$scope",
            "notify",
            "$state",
            "$uibModalInstance"
        ];

        constructor(
            private dataProcessingAgreementService: Services.DataProcessing.IDataProcessingAgreementService,
            private user: Services.IUser,
            private $scope,
            private notify,
            private $state: angular.ui.IStateService,
            private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance) {
        }

        public type: string = "Databehandleraftale";
        public checkAvailableUrl: string = "api/v1/data-processing-agreement";

        private createNew() {
            let name = this.$scope.formData.name;
            let organizationId = this.user.currentOrganizationId;

            var msg = this.notify.addInfoMessage('Opretter databehandleraftale...', false);

            return this
                .dataProcessingAgreementService
                .create(organizationId, name)
                .then
                (
                    createdResponse => {
                        if (createdResponse.created) {
                            msg.toSuccessMessage("En ny databehandleraftale er oprettet!");
                        } else {
                            msg.toErrorMessage("Fejl! Kunne ikke oprette ny databehandleraftale!");
                        }
                        return createdResponse;
                    }
                );
        }

        save(): void {
            this.createNew()
                .then(response => {
                    if (response.created) {
                        this.$uibModalInstance.close();
                    }
                });
        }

        saveAndProceed(): void {
            this.createNew()
                .then(response => {
                    if (response.created) {
                        this.$uibModalInstance.close();
                        this.$state.go("data-processing.overview.edit-agreement.main", { id: response.createdObjectId });
                    }
                });
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.overview.create-agreement", {
                url: "/create-agreement",
                onEnter: [
                    "$state", "$uibModal",
                    ($state: ng.ui.IStateService, $uibModal: ng.ui.bootstrap.IModalService) => {
                        $uibModal.open({
                            templateUrl: "app/components/data-processing/data-processing-agreement-modal-create.view.html",
                            windowClass: "modal fade in",
                            resolve: {
                                user: ["userService", (userService: Services.IUserService) => userService.getUser()]
                            },
                            controller: CreateDateProcessingAgreementController,
                            controllerAs: "vm",
                        }).result.then(() => {
                            $state.go("^", null, { reload: true });
                        },
                            () => {
                                $state.go("^");
                            });
                    }
                ]
            });
        }]);
}
