﻿module Kitos.DataProcessing.Registration.Edit.Main {
    "use strict";

    export class EditMainDataProcessingRegistrationController {
        static $inject: Array<string> = [
            "dataProcessingRegistrationService",
            "hasWriteAccess",
            "dataProcessingRegistration",
            "apiUseCaseFactory",
            "select2LoadingService",
            "notify",
            "dataProcessingRegistrationOptions",
            "bindingService",
            "subDataProcessorDialogFactory"
        ];

        private readonly dataProcessingRegistrationId: number;
        constructor(
            private readonly dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService,
            public hasWriteAccess,
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory,
            private readonly select2LoadingService: Services.ISelect2LoadingService,
            private readonly notify,
            private readonly dataProcessingRegistrationOptions: Models.DataProcessing.IDataProcessingRegistrationOptions,
            private readonly bindingService: Services.Generic.IBindingService,
            private readonly subDataProcessorDialogFactory: Edit.SubDataProcessor.ISubDataProcessorDialogFactory) {
            this.dataProcessingRegistrationId = this.dataProcessingRegistration.id;
            this.loadState();
        }

        private loadState() {
            this.bindDataProcessors();
            this.bindSubDataProcessors();
            this.bindHasSubDataProcessors();
            this.bindIsAgreementConcluded();
            this.bindAgreementConcludedRemark();
            this.bindAgreementConcludedAt();
            this.bindTransferToInsecureThirdCountries();
            this.bindBasisForTransfer();
            this.bindDataResponsible();
            this.bindDataResponsibleRemark();
            this.reloadValidationStatus();
        }

        headerName = this.dataProcessingRegistration.name;

        lastChangedBy = this.dataProcessingRegistration.lastChangedByName;

        lastChangedAt = Kitos.Helpers.RenderFieldsHelper.renderDate(this.dataProcessingRegistration.lastChangedAt);

        insecureThirdCountries: Models.ViewModel.Generic.IMultipleSelectionWithSelect2ConfigViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>;

        enableSelectionOfInsecureThirdCountries: boolean;

        transferToInsecureThirdCountries: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YesNoUndecidedOption>;

        dataProcessors: Models.ViewModel.Generic.IMultipleSelectionWithSelect2ConfigViewModel<Models.DataProcessing.IDataProcessorDTO>;

        subDataProcessors: Models.ViewModel.Generic.IMultipleSelectionWithSelect2ConfigViewModel<Models.DataProcessing.IDataProcessorDTO>;

        isAgreementConcluded: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YesNoIrrelevantOption>;

        agreementConcludedAt: Models.ViewModel.Generic.IDateSelectionViewModel;

        agreementConcludedRemark: Models.ViewModel.Generic.IEditTextViewModel;

        shouldShowAgreementConcludedAt: boolean;

        enableDataProcessorSelection: boolean;

        hasSubDataProcessors: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YesNoUndecidedOption>;

        basisForTransfer: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>;

        dataResponsible: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>;

        dataResponsibleRemark: Models.ViewModel.Generic.IEditTextViewModel;

        validationStatus: Models.DataProcessing.IDataProcessingRegistrationValidationDTO;

        reloadValidationStatus() {
            this.dataProcessingRegistrationService.getValidationDetails(this.dataProcessingRegistration.id).then((newStatus: Models.DataProcessing.IDataProcessingRegistrationValidationDTO) => {
                this.validationStatus = newStatus;
            });
        }
        
        yesNoUndecidedOptionsViewModel = Models.ViewModel.Shared.YesNoUndecidedOptions;

        createSubDataProcessor() {
            this.openSubDataProcessorModal();
        }

        updateSubDataProcessor(id: number) {
            this.openSubDataProcessorModal(id);
        }

        changeName(name) {
            this.apiUseCaseFactory
                .createUpdate("Navn", () => this.dataProcessingRegistrationService.rename(this.dataProcessingRegistrationId, name))
                .executeAsync(nameChangeResponse => {
                    this.headerName = name;
                    return nameChangeResponse;
                });
        }

        removeSubDataProcessor(id: number) {
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

        private openSubDataProcessorModal(subDataProcessorId: number = null) {
            this.subDataProcessorDialogFactory.open(subDataProcessorId,
                this.dataProcessingRegistration,
                this.dataProcessingRegistrationOptions)
                .result.then((isSubProcessorChanged: boolean) => {
                    //Reload state from backend if the dialog was closed 
                    if (isSubProcessorChanged) {
                        this.bindSubDataProcessors();
                    }
                });
        }

        private bindDataResponsible() {
            this.dataResponsible =
                Helpers.Select2MappingHelper.createNewNamedEntityWithDescriptionAndExpirationStatusDtoViewModel(
                    this.dataProcessingRegistration.dataResponsible.value,
                    this.dataProcessingRegistrationOptions.dataResponsibleOptions,
                    (newElement) => this.updateDataResponsible(newElement),
                    this.select2LoadingService);
        }

        private bindDataResponsibleRemark() {
            this.dataResponsibleRemark = new Models.ViewModel.Generic.EditTextViewModel(
                this.dataProcessingRegistration.dataResponsible.remark,
                (newText) => this.changeDataResponsibleRemark(newText));
        }

        private bindAgreementConcludedRemark() {
            this.agreementConcludedRemark = new Models.ViewModel.Generic.EditTextViewModel(
                this.dataProcessingRegistration.agreementConcluded.remark,
                (newText) => this.changeAgreementConcludedRemark(newText));
        }

        private bindBasisForTransfer() {
            this.basisForTransfer = Helpers.Select2MappingHelper.createNewNamedEntityWithDescriptionAndExpirationStatusDtoViewModel(
                this.dataProcessingRegistration.basisForTransfer,
                this.dataProcessingRegistrationOptions.basisForTransferOptions,
                (newElement) => this.updateBasisForTransfer(newElement),
                this.select2LoadingService,
                true,
                false); //We only allow selection of non-expired and this object is based on the available objects
        }

        private bindTransferToInsecureThirdCountries() {

            const options = new Models.ViewModel.Shared.YesNoUndecidedOptions();
            this.transferToInsecureThirdCountries = {

                selectedElement: options.getById(this.dataProcessingRegistration.transferToInsecureThirdCountries),
                select2Config: this.select2LoadingService.select2LocalDataNoSearch(() => options.options, false),
                elementSelected: (newElement) => {
                    if (!!newElement) {
                        this.changeTransferToInsecureThirdCountries(newElement.optionalObjectContext);
                    }
                }
            };
            this.enableSelectionOfInsecureThirdCountries = this.dataProcessingRegistration.transferToInsecureThirdCountries === Models.Api.Shared.YesNoUndecidedOption.Yes;

            this.bindingService.bindMultiSelectConfiguration<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>(
                config => this.insecureThirdCountries = config,
                () => this.dataProcessingRegistration.insecureThirdCountries.sort((a, b) => a.name.localeCompare(b.name, Kitos.Shared.Localization.danishLocale)),
                element => this.removeInsecureThirdCountry(element.id),
                newElement => this.addInsecureThirdCountry(newElement),
                this.hasWriteAccess,
                this.hasWriteAccess,
                null,
                () => {
                    const selectedCountries = this
                        .dataProcessingRegistration
                        .insecureThirdCountries
                        .reduce((acc, next, _) => {
                            acc[next.id] = next;
                            return acc;
                        },
                            {});
                    return this.dataProcessingRegistrationOptions.thirdCountryOptions.filter(x => !selectedCountries[x.id]).map(x => {
                        return {
                            text: x.name,
                            id: x.id,
                            optionalObjectContext: {
                                id: x.id,
                                name: x.name,
                                description: x.description,
                                expired: false //We only allow selection of non-expired and this object is based on the available objects
                            }
                        };
                    });
                },
                null,
                true
            );
        }

        private bindHasSubDataProcessors() {
            const options = new Models.ViewModel.Shared.YesNoUndecidedOptions();
            this.hasSubDataProcessors = {
                selectedElement: options.getById(this.dataProcessingRegistration.hasSubDataProcessors),
                select2Config: this.select2LoadingService.select2LocalDataNoSearch(() => options.options, false),
                elementSelected: (newElement) => {
                    if (!!newElement) {
                        const changeFromYes = this.dataProcessingRegistration.hasSubDataProcessors === Models.Api.Shared.YesNoUndecidedOption.Yes && newElement.optionalObjectContext !== Models.Api.Shared.YesNoUndecidedOption.Yes;
                        if (changeFromYes) {
                            if (this.dataProcessingRegistration?.subDataProcessors?.length > 0) {
                                if (!confirm("Alle registreringer for underdatabehandlinger fjernes. Vil du fortætte?")) {
                                    return;
                                }
                            }
                        }
                        this.changeHasSubDataProcessor(newElement.optionalObjectContext);
                    }
                }
            };
            this.enableDataProcessorSelection = this.dataProcessingRegistration.hasSubDataProcessors === Models.Api.Shared.YesNoUndecidedOption.Yes;
        }
        
        private bindDataProcessors() {
            const pageSize = 100;
            this.bindingService.bindMultiSelectConfiguration<Models.DataProcessing.IDataProcessorDTO>(
                config => this.dataProcessors = config,
                () => this.dataProcessingRegistration.dataProcessors.sort((a, b) => a.name.localeCompare(b.name, Shared.Localization.danishLocale)),
                element => this.removeDataProcessor(element.id),
                newElement => this.addDataProcessor(newElement),
                this.hasWriteAccess,
                this.hasWriteAccess,
                (query) => this.dataProcessingRegistrationService.getApplicableDataProcessors(this.dataProcessingRegistrationId, query, pageSize)
                    .then(results => Helpers.Select2MappingHelper.mapDataProcessingSearchResults(results)),
                null,
                Helpers.Select2OptionsFormatHelper.formatOrganizationWithOptionalObjectContext
            );
        }

        private bindSubDataProcessors() {
            this.dataProcessingRegistrationService.get(this.dataProcessingRegistration.id)
                .then(reloadedSubDp => {
                    this.dataProcessingRegistration.subDataProcessors = reloadedSubDp.subDataProcessors.sort((a, b) => a.name.localeCompare(b.name, Shared.Localization.danishLocale));
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

        private removeInsecureThirdCountry(id: number) {
            this.apiUseCaseFactory
                .createAssignmentRemoval(() => this.dataProcessingRegistrationService.removeInsecureThirdCountry(this.dataProcessingRegistrationId, id))
                .executeAsync(success => {

                    //Update the source collection
                    this.dataProcessingRegistration.insecureThirdCountries = this.dataProcessingRegistration.insecureThirdCountries.filter(x => x.id !== id);

                    //Propagate changes to UI binding
                    this.bindTransferToInsecureThirdCountries();
                    return success;
                });
        }

        private addInsecureThirdCountry(newElement: Models.ViewModel.Generic.Select2OptionViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>) {
            if (!!newElement && !!newElement.optionalObjectContext) {
                const country = newElement.optionalObjectContext as Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO;
                this.apiUseCaseFactory
                    .createAssignmentCreation(() => this.dataProcessingRegistrationService.assignInsecureThirdCountry(this.dataProcessingRegistrationId, country.id))
                    .executeAsync(success => {
                        //Update the source collection
                        this.dataProcessingRegistration.insecureThirdCountries.push(country);

                        //Trigger UI update
                        this.bindTransferToInsecureThirdCountries();
                        return success;
                    });
            }
        }

        private updateBasisForTransfer(newValue?: Models.ViewModel.Generic.Select2OptionViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>) {

            const updateFunc = newValue
                ? () => this.dataProcessingRegistrationService.assignBasisForTransfer(this.dataProcessingRegistration.id, newValue.id)
                : () => this.dataProcessingRegistrationService.clearBasisForTransfer(this.dataProcessingRegistration.id);

            this.apiUseCaseFactory
                .createUpdate("Overførselsgrundlag", () => updateFunc())
                .executeAsync(success => {
                    this.dataProcessingRegistration.basisForTransfer = newValue && newValue.optionalObjectContext;
                    this.bindBasisForTransfer();
                    return success;
                });
        }

        private updateDataResponsible(newValue?: Models.ViewModel.Generic.Select2OptionViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>) {

            const updateFunc = newValue
                ? () => this.dataProcessingRegistrationService.assignDataResponsible(this.dataProcessingRegistration.id, newValue.id)
                : () => this.dataProcessingRegistrationService.clearDataResponsible(this.dataProcessingRegistration.id);

            this.apiUseCaseFactory
                .createUpdate("Dataansvarlig", () => updateFunc())
                .executeAsync(success => {
                    this.dataProcessingRegistration.dataResponsible.value = newValue && newValue.optionalObjectContext;
                    this.bindBasisForTransfer();
                    return success;
                });
        }

        private changeDataResponsibleRemark(oversightIntervalRemark: string) {
            this.apiUseCaseFactory
                .createUpdate("Bemærkning", () => this.dataProcessingRegistrationService.updateDataResponsibleRemark(this.dataProcessingRegistration.id, oversightIntervalRemark))
                .executeAsync(success => {
                    this.dataProcessingRegistration.dataResponsible.remark = oversightIntervalRemark;
                    this.bindDataResponsibleRemark();
                    return success;
                });
        }

        //AgreementConcludedRemark
        private changeAgreementConcludedRemark(agreementConcludedRemark: string) {
            this.apiUseCaseFactory
                .createUpdate("Bemærkning", () => this.dataProcessingRegistrationService.updateAgreementConcludedRemark(this.dataProcessingRegistration.id, agreementConcludedRemark))
                .executeAsync(success => {
                    this.dataProcessingRegistration.agreementConcluded.remark = agreementConcludedRemark;
                    this.bindAgreementConcludedRemark();
                    return success;
                });
        }


        private changeIsAgreementConcluded(isAgreementConcluded: Models.ViewModel.Generic.Select2OptionViewModel<Models.Api.Shared.YesNoIrrelevantOption>) {
            this.apiUseCaseFactory
                .createUpdate("Databehandleraftale indgået", () => this.dataProcessingRegistrationService.updateIsAgreementConcluded(this.dataProcessingRegistration.id, isAgreementConcluded.optionalObjectContext))
                .executeAsync(success => {
                    if (success.optionalServerDataPush) {
                        this.dataProcessingRegistration.agreementConcluded = success.optionalServerDataPush.agreementConcluded;
                    }
                    this.dataProcessingRegistration.agreementConcluded.value = isAgreementConcluded.optionalObjectContext;
                    this.bindIsAgreementConcluded();
                    this.bindAgreementConcludedAt();
                    return success;
                });
        }

        private changeHasSubDataProcessor(value: Models.Api.Shared.YesNoUndecidedOption) {
            this
                .apiUseCaseFactory
                .createUpdate("Underdatabehandlere", () => this.dataProcessingRegistrationService.updateSubDataProcessorsState(this.dataProcessingRegistrationId, value))
                .executeAsync(success => {
                    this.dataProcessingRegistration.hasSubDataProcessors = value;
                    if (success.optionalServerDataPush) {
                        this.dataProcessingRegistration.subDataProcessors = success.optionalServerDataPush.subDataProcessors;
                    }
                    this.bindHasSubDataProcessors();
                    this.bindSubDataProcessors();
                    return success;
                });
        }

        private changeTransferToInsecureThirdCountries(value: Models.Api.Shared.YesNoUndecidedOption) {
            this
                .apiUseCaseFactory
                .createUpdate("Overførsel til usikkert 3. land", () => this.dataProcessingRegistrationService.updateTransferToInsecureThirdCountry(this.dataProcessingRegistrationId, value))
                .executeAsync(success => {
                    if (success.optionalServerDataPush) {
                        this.dataProcessingRegistration.insecureThirdCountries = success.optionalServerDataPush.insecureThirdCountries;
                    }
                    this.dataProcessingRegistration.transferToInsecureThirdCountries = value;
                    this.bindTransferToInsecureThirdCountries();
                    return success;
                });
        }

        private changeAgreementConcludedAt(agreementConcludedAt: string) {
            var formattedDate = Helpers.DateStringFormat.fromDDMMYYYYToYYYYMMDD(agreementConcludedAt);
            if (!!formattedDate.convertedValue) {
                return this.apiUseCaseFactory
                    .createUpdate("Dato for indgåelse af databehandleraftale", () => this.dataProcessingRegistrationService.updateAgreementConcludedAt(this.dataProcessingRegistration.id, formattedDate.convertedValue))
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
            return new Models.ViewModel.Shared.YesNoIrrelevantOptions().getById(id);
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
                controllerAs: "vm",
                resolve: {
                }
            });
        }]);
}
