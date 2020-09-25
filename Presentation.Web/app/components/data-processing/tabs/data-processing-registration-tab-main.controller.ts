module Kitos.DataProcessing.Registration.Edit.Main {
    "use strict";

    export class EditMainDataProcessingRegistrationController {
        static $inject: Array<string> = [
            "dataProcessingRegistrationService",
            "hasWriteAccess",
            "dataProcessingRegistration",
            "apiUseCaseFactory",
            "select2LoadingService",
            "notify"
        ];

        private readonly dataProcessingRegistrationId: number;

        constructor(
            private readonly dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService,
            public hasWriteAccess,
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory,
            private readonly select2LoadingService: Services.ISelect2LoadingService,
            private readonly notify) {
            this.bindDataProcessors();
            this.bindSubDataProcessors();
            this.bindHasSubDataProcessors();
            this.dataProcessingRegistrationId = this.dataProcessingRegistration.id;
            this.bindIsAgreementConcluded();
            this.bindAgreementConcludedAt();
        }

        headerName = this.dataProcessingRegistration.name;

        dataProcessors: Models.ViewModel.Generic.IMultipleSelectionWithSelect2ConfigViewModel<Models.DataProcessing.IDataProcessorDTO>;

        subDataProcessors: Models.ViewModel.Generic.IMultipleSelectionWithSelect2ConfigViewModel<Models.DataProcessing.IDataProcessorDTO>;

        isAgreementConcluded: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YesNoIrrelevantOption>;        

        agreementConcludedAt: Models.ViewModel.Generic.IDateSelectionViewModel;

        shouldShowAgreementConcludedAt: boolean;

        enableDataProcessorSelection: boolean;

        hasSubDataProcessors: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YesNoUndecidedOption>;

        private bindHasSubDataProcessors() {
            this.hasSubDataProcessors = {
                selectedElement: this.dataProcessingRegistration.hasSubDataProcessors !== null && new Models.ViewModel.Shared.YesNoUndecidedOptions().options.filter(x=>x.id === (this.dataProcessingRegistration.hasSubDataProcessors as number))[0],
                select2Config: this.select2LoadingService.select2LocalDataNoSearch(() => new Models.ViewModel.Shared.YesNoUndecidedOptions().options, false),
                elementSelected: (newElement) => this.changeHasSubDataProcessor(newElement.optionalObjectContext)
            };
            this.enableDataProcessorSelection = this.dataProcessingRegistration.hasSubDataProcessors === Models.Api.Shared.YesNoUndecidedOption.Yes;
        }

        private bindMultiSelectConfiguration<TElement>(
            setField: ((finalVm: Models.ViewModel.Generic.IMultipleSelectionWithSelect2ConfigViewModel<TElement>) => void),
            getInitialElements: () => TElement[],
            removeFunc: ((element: TElement) => void),
            newFunc: Models.ViewModel.Generic.NewItemSelectedFunc,
            searchFunc: (query: string) => angular.IPromise<Models.ViewModel.Generic.Select2OptionViewModel<TElement>[]>) {

            const configuration = {
                selectedElements: getInitialElements(),
                removeItemRequested: removeFunc,
                allowAddition: this.hasWriteAccess,
                allowRemoval: this.hasWriteAccess,
                newElementSelection: null,
                select2Config: this
                    .select2LoadingService
                    .loadSelect2WithDataSource(searchFunc, false),
                newItemSelected: newFunc
            };
            setField(configuration);
        }

        private mapDataProcessingSearchResults(dataProcessors: Models.DataProcessing.IDataProcessorDTO[]) {
            return dataProcessors.map(
                dataProcessor => <Models.ViewModel.Generic.Select2OptionViewModel<Models.DataProcessing.IDataProcessorDTO>>{
                    id: dataProcessor.id,
                    text: dataProcessor.name,
                    optionalObjectContext: dataProcessor
                }
            );
        }

        private bindDataProcessors() {
            this.bindMultiSelectConfiguration<Models.DataProcessing.IDataProcessorDTO>(
                config => this.dataProcessors = config,
                () => this.dataProcessingRegistration.dataProcessors,
                element => this.removeDataProcessor(element.id),
                newElement => this.addDataProcessor(newElement),
                (query) => this
                    .dataProcessingRegistrationService
                    .getApplicableDataProcessors(this.dataProcessingRegistrationId, query)
                    .then(results => this.mapDataProcessingSearchResults(results))
            );
        }
        private bindSubDataProcessors() {
            this.bindMultiSelectConfiguration<Models.DataProcessing.IDataProcessorDTO>(
                config => this.subDataProcessors = config,
                () => this.dataProcessingRegistration.subDataProcessors,
                element => this.removeSubDataProcessor(element.id),
                newElement => this.addSubDataProcessor(newElement),
                (query) => this
                    .dataProcessingRegistrationService
                    .getApplicableSubDataProcessors(this.dataProcessingRegistrationId, query)
                    .then(results => this.mapDataProcessingSearchResults(results))
            );
        }

        changeName(name) {
            this.apiUseCaseFactory
                .createUpdate("Navn", () => this.dataProcessingRegistrationService.rename(this.dataProcessingRegistrationId, name))
                .executeAsync(nameChangeResponse => {
                    this.headerName = name;
                    return nameChangeResponse;
                });
        }

        private addDataProcessor(newElement: Models.ViewModel.Generic.Select2OptionViewModel<Models.DataProcessing.IDataProcessorDTO>) {
            if (!!newElement && !!newElement.optionalObjectContext) {
                const newDp = newElement.optionalObjectContext;
                this.apiUseCaseFactory
                    .createAssignmentCreation(() => this.dataProcessingRegistrationService.assignDataProcessor(this.dataProcessingRegistrationId, newDp.id))
                    .executeAsync(success => {
                        //Update the source collection
                        this.dataProcessingRegistration.dataProcessors.push(newDp);

                        //Trigger UI update
                        this.bindDataProcessors();
                        return success;
                    });
            }
        }

        private removeDataProcessor(id: number) {
            this.apiUseCaseFactory
                .createAssignmentRemoval(() => this.dataProcessingRegistrationService.removeDataProcessor(this.dataProcessingRegistrationId, id))
                .executeAsync(success => {

                    //Update the source collection
                    this.dataProcessingRegistration.dataProcessors = this.dataProcessingRegistration.dataProcessors.filter(x => x.id !== id);

                    //Propagate changes to UI binding
                    this.bindDataProcessors();
                    return success;
                });
        }

        private addSubDataProcessor(newElement: Models.ViewModel.Generic.Select2OptionViewModel<Models.DataProcessing.IDataProcessorDTO>) {
            if (!!newElement && !!newElement.optionalObjectContext) {
                const newDp = newElement.optionalObjectContext as Models.DataProcessing.IDataProcessorDTO;
                this.apiUseCaseFactory
                    .createAssignmentCreation(() => this.dataProcessingRegistrationService.assignSubDataProcessor(this.dataProcessingRegistrationId, newDp.id))
                    .executeAsync(success => {
                        //Update the source collection
                        this.dataProcessingRegistration.subDataProcessors.push(newDp);

                        //Trigger UI update
                        this.bindSubDataProcessors();
                        return success;
                    });
            }
        }

        private removeSubDataProcessor(id: number) {
            this.apiUseCaseFactory
                .createAssignmentRemoval(() => this.dataProcessingRegistrationService.removeSubDataProcessor(this.dataProcessingRegistrationId, id))
                .executeAsync(success => {

                    //Update the source collection
                    this.dataProcessingRegistration.subDataProcessors = this.dataProcessingRegistration.subDataProcessors.filter(x => x.id !== id);

                    //Propagate changes to UI binding
                    this.bindSubDataProcessors();
                    return success;
                });
        }

        private changeIsAgreementConcluded(isAgreementConcluded: Models.ViewModel.Generic.Select2OptionViewModel<Models.Api.Shared.YesNoIrrelevantOption>) {
            this.apiUseCaseFactory
                .createUpdate("Databehandleraftale indgået", () => this.dataProcessingRegistrationService.updateIsAgreementConcluded(this.dataProcessingRegistration.id, isAgreementConcluded.optionalObjectContext))
                .executeAsync(success => {
                    this.dataProcessingRegistration.agreementConcluded.value = isAgreementConcluded.optionalObjectContext;
                    this.bindIsAgreementConcluded();
                    return success;
                });
        }

        private changeHasSubDataProcessor(value: Models.Api.Shared.YesNoUndecidedOption) {
            this
                .apiUseCaseFactory
                .createUpdate("Underdatabehandlere", () => this.dataProcessingRegistrationService.updateSubDataProcessorsState(this.dataProcessingRegistrationId, value))
                .executeAsync(success => {
                    this.dataProcessingRegistration.hasSubDataProcessors = value;
                    this.bindHasSubDataProcessors();
                    return success;
                });
        }

        private changeAgreementConcludedAt(agreementConcludedAt: string) {
            var formattedDate = Helpers.DateStringFormat.fromDDMMYYYYToYYYYMMDD(agreementConcludedAt);
            if (!!formattedDate.convertedValue) {
                return this.apiUseCaseFactory
                    .createUpdate("Dato for databehandleraftale indgået", () => this.dataProcessingRegistrationService.updateAgreementConcludedAt(this.dataProcessingRegistration.id, formattedDate.convertedValue))
                    .executeAsync(success => {
                        this.dataProcessingRegistration.agreementConcluded.optionalDateValue = agreementConcludedAt;
                        this.bindAgreementConcludedAt();
                        return success;
                    });
            }
            return this.notify.addErrorMessage(formattedDate.errorMessage);            
        }

        private bindIsAgreementConcluded() {
            this.isAgreementConcluded = {
                selectedElement: this.getYesNoIrrelevantOptionFromId(this.dataProcessingRegistration.agreementConcluded.value),
                select2Config: this.select2LoadingService.select2LocalDataNoSearch(() => new Models.ViewModel.Shared.YesNoIrrelevantOptions().options, false),
                elementSelected: (newElement) => this.changeIsAgreementConcluded(newElement)
            };

            this.shouldShowAgreementConcludedAt =
                this.isAgreementConcluded.selectedElement !== null &&
                this.isAgreementConcluded.selectedElement.optionalObjectContext === Models.Api.Shared.YesNoIrrelevantOption.YES;
        }

        private getYesNoIrrelevantOptionFromId(id?: number): Models.ViewModel.Generic.Select2OptionViewModel<Models.Api.Shared.YesNoIrrelevantOption> {
            if (id === null) {
                return null;
            }
            return new Models.ViewModel.Shared.YesNoIrrelevantOptions().options.filter(option => option.id === id)[0];
        }

        private bindAgreementConcludedAt() {
            this.agreementConcludedAt = new Models.ViewModel.Generic.DateSelectionViewModel(
                this.dataProcessingRegistration.agreementConcluded.optionalDateValue,
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
