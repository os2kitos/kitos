module Kitos.ItContract.Create {
    "use strict";

    export class CreateItContractController {
        static $inject: Array<string> = [
            "user",
            "$http",
            "$scope",
            "notify",
            "$state",
            "$uibModalInstance"
        ];

        constructor(
            private readonly user: Services.IUser,
            private readonly $http: ng.IHttpService,
            private readonly $scope,
            private readonly notify,
            private readonly $state: angular.ui.IStateService,
            private readonly $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance) {
        }

        readonly type: string = "IT Kontrakt";

        private createNew() {
            const name = this.$scope.formData.name;
            const organizationId = this.user.currentOrganizationId;

            var msg = this.notify.addInfoMessage('Opretter kontrakt...', false);

            return this.$http.post(`api/itcontract?organizationId=${this.user.currentOrganizationId}`,
                { organizationId: organizationId, name: name })
                .then
                (
                    (createdResponse: any) => {
                        msg.toSuccessMessage("En ny kontrakt er oprettet!");
                        return createdResponse;
                    },
                    (errorResponse: Models.Api.ApiResponseErrorCategory) => {
                        switch (errorResponse) {
                        //TODO: check if the error messages are valid
                            case Models.Api.ApiResponseErrorCategory.BadInput:
                                msg.toErrorMessage("Fejl! Navnet er ugyldigt!");
                                break;
                            case Models.Api.ApiResponseErrorCategory.Conflict:
                                msg.toErrorMessage("Fejl! Navnet er allerede brugt!");
                                break;
                            default:
                                msg.toErrorMessage("Fejl! Kunne ikke oprette en ny kontrakt!");
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
                .then(result => {
                    if (result) {
                        this.close();
                        this.popState();
                        const contract = result.data.response;
                        this.$state.go("it-contract.edit.main", { id: contract.id });
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
            $stateProvider.state("it-contract.overview.create", {
                url: "/create-contract",
                onEnter: [
                    "$state", "$uibModal",
                    ($state: ng.ui.IStateService, $uibModal: ng.ui.bootstrap.IModalService) => {
                        $uibModal.open({
                            templateUrl: "app/components/it-contract/it-contract-modal-create.view.html",
                            windowClass: "modal fade in",
                            resolve: {
                                user: ["userService", (userService: Services.IUserService) => userService.getUser()]
                            },
                            controller: CreateItContractController,
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