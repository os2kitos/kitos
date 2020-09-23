﻿module Kitos.DataProcessing.Registration.Edit.Main {
    "use strict";

    export class EditMainDataProcessingRegistrationController {
        static $inject: Array<string> = [
            "dataProcessingRegistrationService",
            "hasWriteAccess",
            "dataProcessingRegistration",
            "apiUseCaseFactory"
        ];


        constructor(
            private readonly dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService,
            public hasWriteAccess,
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory) {

            this.bindIsAgreementConcluded();
            this.bindAgreementConcludedAt();
        }

        headerName = this.dataProcessingRegistration.name;

        isAgreementConcluded: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<number>;

        agreementConcludedAt: Models.ViewModel.Generic.IDateSelectionViewModel
        
        changeName(name) {
            this.apiUseCaseFactory
                .createUpdate(() => this.dataProcessingRegistrationService.rename(this.dataProcessingRegistration.id, name))
                .executeAsync(nameChangeResponse => this.headerName = nameChangeResponse.valueModifiedTo);
        }

        changeIsAgreementConcluded(isAgreementConcluded) {
            this.apiUseCaseFactory
                .createUpdate(() => this.dataProcessingRegistrationService.updateIsAgreementConcluded(this.dataProcessingRegistration.id, isAgreementConcluded))
                .executeAsync();
        }

        changeAgreementConcludedAt(agreementConcludedAt) {
            this.apiUseCaseFactory
                .createUpdate(() => this.dataProcessingRegistrationService.updateAgreementConcludedAt(this.dataProcessingRegistration.id, agreementConcludedAt))
                .executeAsync();
        }


        private bindIsAgreementConcluded() {
            this.isAgreementConcluded = {
                selectedElement: this.dataProcessingRegistration.isAgreementConcluded,
                select2Options: new Models.ViewModel.DataProcessingAgreement.AgreementConcludedOptions().options,
                elementSelected: (newElement) => this.changeIsAgreementConcluded(newElement)
            };
        }

        private bindAgreementConcludedAt() {
            this.agreementConcludedAt = new Models.ViewModel.Generic.DateSelectionViewModel(
                this.dataProcessingRegistration.agreementConcludedAt,
                (newDate) => this.changeAgreementConcludedAt(newDate));
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.edit-registration.main", {
                url: "/main",
                templateUrl: "app/components/data-processing/tabs/data-processing-registration-tab-main.view.html",
                controller: EditMainDataProcessingRegistrationController,
                controllerAs: "vm"
            });
        }]);
}
