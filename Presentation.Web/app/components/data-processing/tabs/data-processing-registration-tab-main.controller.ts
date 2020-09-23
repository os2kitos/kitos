module Kitos.DataProcessing.Registration.Edit.Main {
    "use strict";

    export class EditMainDataProcessingRegistrationController {
        static $inject: Array<string> = [
            "dataProcessingRegistrationService",
            "hasWriteAccess",
            "dataProcessingRegistration",
            "apiUseCaseFactory",
            "select2LoadingService"
        ];


        constructor(
            private readonly dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService,
            public hasWriteAccess,
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory,
            private readonly select2LoadingService: Services.ISelect2LoadingService) {
            this.bindDataProcessors();

            this.bindIsAgreementConcluded();
            this.bindAgreementConcludedAt();
        }

        headerName = this.dataProcessingRegistration.name;

        dataProcessors: Models.ViewModel.Generic.IMultipleSelectionWithSelect2ConfigViewModel<Models.DataProcessing.IDataProcessorDTO>;

        private bindDataProcessors() {
            this.dataProcessors = {
                selectedElements: this.dataProcessingRegistration.dataProcessors,
                removeItemRequested: (element) => this.removeDataProcessor(element.id),
                allowAddition: this.hasWriteAccess,
                allowRemoval: this.hasWriteAccess,
                newElementSelection: null,
                select2Config: this
                    .select2LoadingService
                    .loadSelect2WithDataSource(
                        (query) => this
                            .dataProcessingRegistrationService
                            .getApplicableDataProcessors(this.dataProcessingRegistration.id, query)
                            .then(
                                dataProcessors => dataProcessors.map(
                                    dataProcessor => <Models.ViewModel.Generic.Select2OptionViewModel>{
                                        id: dataProcessor.id,
                                        text: dataProcessor.name,
                                        optionalObjectContext: dataProcessor
                                    }
                                )
                            ),
                        false
                    ),
                newItemSelected: (newElement) => this.addDataProcessor(newElement)
            };
        }

        changeName(name) {
        
            this.apiUseCaseFactory
                .createUpdate("Navn", () => this.dataProcessingRegistrationService.rename(this.dataProcessingRegistration.id, name))
                .executeAsync(nameChangeResponse => {
                    this.headerName = name;
                    return nameChangeResponse;
                });
        }

        isAgreementConcluded: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<number>;

        private addDataProcessor(newElement: Models.ViewModel.Generic.Select2OptionViewModel) {
            if (!!newElement && !!newElement.optionalObjectContext) {
                const newDp = newElement.optionalObjectContext as Models.DataProcessing.IDataProcessorDTO;
                this.apiUseCaseFactory
                    .createAssignmentCreation(() => this.dataProcessingRegistrationService.assignDataProcessor(this.dataProcessingRegistration.id, newDp.id))
                    .executeAsync(success => {
                        //Update the source collection
                        this.dataProcessingRegistration.dataProcessors.push(newDp);

                        //Trigger UI update
                        this.bindDataProcessors();
                        return success;
                    });
            }
        }

        agreementConcludedAt: Models.ViewModel.Generic.IDateSelectionViewModel
        
        private removeDataProcessor(id: number) {
            this.apiUseCaseFactory
                .createAssignmentRemoval(() => this.dataProcessingRegistrationService.removeDataProcessor(this.dataProcessingRegistration.id, id))
                .executeAsync(success => {

                    //Update the source collection
                    this.dataProcessingRegistration.dataProcessors = this.dataProcessingRegistration.dataProcessors.filter(x => x.id !== id);

                    //Propagate changes to UI binding
                    this.bindDataProcessors();
                    return success;
                });
        }

        changeIsAgreementConcluded(isAgreementConcluded) {
            this.apiUseCaseFactory
                .createUpdate("Databehandleraftale indgået", () => this.dataProcessingRegistrationService.updateIsAgreementConcluded(this.dataProcessingRegistration.id, isAgreementConcluded))
                .executeAsync();
        }

        changeAgreementConcludedAt(agreementConcludedAt) {
            this.apiUseCaseFactory
                .createUpdate("Dato for databehandleraftale indgået", () => this.dataProcessingRegistrationService.updateAgreementConcludedAt(this.dataProcessingRegistration.id, agreementConcludedAt))
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
