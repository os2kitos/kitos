module Kitos.DataProcessing.Registration.Create {
    "use strict";

    export class CreateDateProcessingRegistrationController {
        static $inject: Array<string> = [
            "dataProcessingRegistrationService",
            "user",
            "$scope",
            "notify",
            "$state",
            "$uibModalInstance"
        ];

        constructor(
            private readonly dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService,
            private readonly user: Services.IUser,
            private readonly $scope,
            private readonly notify,
            private readonly $state: angular.ui.IStateService,
            private readonly $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance) {
        }

        readonly type: string = "Registrering";

        readonly checkAvailableUrl: string = "api/v1/data-processing-registration";

        private createNew() {
            const name = this.$scope.formData.name;
            const organizationId = this.user.currentOrganizationId;

            var msg = this.notify.addInfoMessage('Opretter registrering...', false);

            return this
                .dataProcessingRegistrationService
                .create(organizationId, name)
                .then
                (
                    createdResponse => {
                        msg.toSuccessMessage("En ny registrering er oprettet!");
                        return createdResponse;
                    },
                    (errorResponse: Models.Api.ApiResponseErrorCategory) => {
                        switch (errorResponse) {
                            case Models.Api.ApiResponseErrorCategory.BadInput:
                                msg.toErrorMessage("Fejl! Navnet er ugyldigt!");
                                break;
                            case Models.Api.ApiResponseErrorCategory.Conflict:
                                msg.toErrorMessage("Fejl! Navnet er allerede brugt!");
                                break;
                            default:
                                msg.toErrorMessage("Fejl! Kunne ikke oprette ny registrering!");
                                break;
                        }

                        //Fail the continuation
                        throw errorResponse;
                    }
                );
        }

        private close() {
            this.$uibModalInstance.close();
        }

        private popState(reload = false) {
            const popped = this.$state.go("^");
            if (reload) {
                popped.then(() => this.$state.reload());
            }

        }

        save(): void {
            this.createNew()
                .then(response => {
                    if (response) {
                        this.close();
                        this.popState(true);
                    }
                });
        }

        saveAndProceed(): void {
            this.createNew()
                .then(response => {
                    if (response) {
                        this.close();
                        this.popState();
                        this.$state.go("data-processing.edit-registration.main", { id: response.createdObjectId });
                    }
                });
        }

        cancel(): void {
            this.close();
            this.popState();
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.overview.create-registration", {
                url: "/create-agreement",
                onEnter: [
                    "$state", "$uibModal",
                    ($state: ng.ui.IStateService, $uibModal: ng.ui.bootstrap.IModalService) => {
                        $uibModal.open({
                            templateUrl: "app/components/data-processing/data-processing-registration-modal-create.view.html",
                            windowClass: "modal fade in",
                            resolve: {
                                user: ["userService", (userService: Services.IUserService) => userService.getUser()]
                            },
                            controller: CreateDateProcessingRegistrationController,
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