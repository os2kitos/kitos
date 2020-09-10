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
            private readonly dataProcessingAgreementService: Services.DataProcessing.IDataProcessingAgreementService,
            private readonly user: Services.IUser,
            private readonly $scope,
            private readonly notify,
            private readonly $state: angular.ui.IStateService,
            private readonly $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance) {
        }

        readonly type: string = "Databehandleraftale";

        readonly checkAvailableUrl: string = "api/v1/data-processing-agreement";

        private createNew() {
            const name = this.$scope.formData.name;
            const organizationId = this.user.currentOrganizationId;

            var msg = this.notify.addInfoMessage('Opretter databehandleraftale...', false);

            return this
                .dataProcessingAgreementService
                .create(organizationId, name)
                .then
                (
                    createdResponse => {
                        msg.toSuccessMessage("En ny databehandleraftale er oprettet!");
                        return createdResponse;
                    },
                    (errorResponse: Models.Api.ApiResponseErrorCategory) => {
                        switch (errorResponse) {
                            case Models.Api.ApiResponseErrorCategory.BadInput:
                                msg.toErrorMessage("Fejl! Navnet var ugyldigt!");
                                break;
                            case Models.Api.ApiResponseErrorCategory.Conflict:
                                msg.toErrorMessage("Fejl! Navnet er allerede brugt!");
                                break;
                            default:
                                msg.toErrorMessage("Fejl! Kunne ikke oprette ny databehandleraftale!");
                                break;
                        }
                    }
                );
        }

        save(): void {
            this.createNew()
                .then(response => {
                    if (response) {
                        this.$uibModalInstance.close();
                    }
                });
        }

        saveAndProceed(): void {
            this.createNew()
                .then(response => {
                    if (response) {
                        this.$uibModalInstance.close();
                        this.$state.go("data-processing.edit-agreement.main", { id: response.createdObjectId });
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
                                // This is handled in Save and saveAndProcess
                            },
                            () => {
                                $state.go("^");
                            });
                    }
                ]
            });
        }]);
}
