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

        private readonly dataProcessingRegistrationId: number;

        constructor(
            private readonly dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService,
            public hasWriteAccess,
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory,
            private readonly select2LoadingService: Services.ISelect2LoadingService) {
            this.bindDataProcessors();
            this.bindSubDataProcessors();
            this.bindHasSubDataProcessors();
            this.dataProcessingRegistrationId = this.dataProcessingRegistration.id;
            this.bindIsAgreementConcluded();
            this.bindAgreementConcludedAt();
        }

        yesNoIrrelevantMapper = Models.Api.Shared.YesNoIrrelevantOptionMapper;

        headerName = this.dataProcessingRegistration.name;

        dataProcessors: Models.ViewModel.Generic.IMultipleSelectionWithSelect2ConfigViewModel<Models.DataProcessing.IDataProcessorDTO>;
        subDataProcessors: Models.ViewModel.Generic.IMultipleSelectionWithSelect2ConfigViewModel<Models.DataProcessing.IDataProcessorDTO>;

        private bindHasSubDataProcessors() {
            this.hasSubDataProcessors = this.dataProcessingRegistration.hasSubDataProcessors as number;
            this.enableDataProcessorSelection = this.dataProcessingRegistration.hasSubDataProcessors === Models.Api.Shared.YesNoUndecidedOption.Yes;
        }

        private bindMultiSelectConfiguration<TElement>(
            setField: ((finalVm: Models.ViewModel.Generic.IMultipleSelectionWithSelect2ConfigViewModel<TElement>) => void),
            getInitialElements: () => TElement[],
            removeFunc: ((element: TElement) => void),
            newFunc: Models.ViewModel.Generic.NewItemSelectedFunc,
            searchFunc: (query: string) => angular.IPromise<Models.ViewModel.Generic.Select2OptionViewModel[]>) {

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
                dataProcessor => <Models.ViewModel.Generic.Select2OptionViewModel>{
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
		
        isAgreementConcluded: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YesNoIrrelevantOption>;

        agreementConcludedAt: Models.ViewModel.Generic.IDateSelectionViewModel;

        shouldShowAgreementConcludedAt(): boolean{
            return this.isAgreementConcluded.selectedElement == Models.Api.Shared.YesNoIrrelevantOption.YES;
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

        private addSubDataProcessor(newElement: Models.ViewModel.Generic.Select2OptionViewModel) {
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

        private changeIsAgreementConcluded(isAgreementConcluded: string) {
            var yesNoIrrelevant = this.yesNoIrrelevantMapper.mapFromIdAsString(isAgreementConcluded);
            this.apiUseCaseFactory
                .createUpdate("Databehandleraftale indgået", () => this.dataProcessingRegistrationService.updateIsAgreementConcluded(this.dataProcessingRegistration.id, yesNoIrrelevant))
                .executeAsync(success => {
                    this.dataProcessingRegistration.agreementConcluded.value = yesNoIrrelevant;
                    this.bindIsAgreementConcluded();
                    return success;
                });
        }

        //TODO: Missing: The flag setting and the "hide/show" logic in the view. Altso refactoring as described in todos above
        //TODO: Use jacobs singleselect model to work to show the select options

        //TODO: Refactor shit below
        enableDataProcessorSelection: boolean;
        hasSubDataProcessors: number;
        changeHasSubDataProcessor(value: string) {
            const valueAsEnum = parseInt(value) as Models.Api.Shared.YesNoUndecidedOption;
            this
                .apiUseCaseFactory
                .createUpdate("Underdatabehandlere", () => this.dataProcessingRegistrationService.updateSubDataProcessorsState(this.dataProcessingRegistrationId, valueAsEnum))
                .executeAsync(success => {
                    this.dataProcessingRegistration.hasSubDataProcessors = valueAsEnum;
                    this.bindHasSubDataProcessors();
					                    return success;
                });
        }

        private changeAgreementConcludedAt(agreementConcludedAt: string) {
            this.apiUseCaseFactory
                .createUpdate("Dato for databehandleraftale indgået", () => this.dataProcessingRegistrationService.updateAgreementConcludedAt(this.dataProcessingRegistration.id, agreementConcludedAt))
                .executeAsync(success => {
                    this.dataProcessingRegistration.agreementConcluded.optionalDateValue = agreementConcludedAt;
                    this.bindAgreementConcludedAt();
                    return success;
                });
        }

        private bindIsAgreementConcluded() {
            this.isAgreementConcluded = {
                selectedElement: this.dataProcessingRegistration.agreementConcluded.value,
                select2Options: new Models.ViewModel.Shared.YesNoIrrelevantOptions().options,
                elementSelected: (newElement) => this.changeIsAgreementConcluded(newElement)
            };
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
