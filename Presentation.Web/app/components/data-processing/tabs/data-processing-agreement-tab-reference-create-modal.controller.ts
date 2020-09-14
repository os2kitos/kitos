module Kitos.DataProcessing.Agreement.Edit.Reference.Create {
    "use strict";

    export class CreateReferenceDataProcessingAgreementController {
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


        //private close() {
        //    this.$uibModalInstance.close();
        //}

        //private popState(reload = false) {
        //    const popped = this.$state.go("^");
        //    if (reload) {
        //        popped.then(() => this.$state.reload());
        //    }

        //}

        //save(): void {
        //    this.createNew()
        //        .then(response => {
        //            if (response) {
        //                this.close();
        //                this.popState(true);
        //            }
        //        });
        //}
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
                            templateUrl: "app/components/it-reference/it-reference-modal.view.html",
                            windowClass: "modal fade in",
                            resolve: {
                                user: ["userService", (userService: Services.IUserService) => userService.getUser()]
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