module Kitos.DataProcessing.Registration.Edit.SubDataProcessor {
    "use strict";
    
    export interface ISubDataProcessorDialogFactory {
        open(subDataProcessorId: number | null, dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO, dataProcessingRegistrationOptions: Models.DataProcessing.IDataProcessingRegistrationOptions): ng.ui.bootstrap.IModalInstanceService
    }

    export class SubDataProcessorDialogFactory implements ISubDataProcessorDialogFactory {
        static $inject = ["$uibModal"];
        constructor(private readonly $uibModal: ng.ui.bootstrap.IModalService) { }

        open(subDataProcessorId: number | null, dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO, dataProcessingRegistrationOptions: Models.DataProcessing.IDataProcessingRegistrationOptions): ng.ui.bootstrap.IModalInstanceService {
            return this.$uibModal.open({
                templateUrl: "app/components/data-processing/tabs/data-processing-registration-sub-data-processor-modal.view.html",
                windowClass: "modal fade in",
                controller: SubDataProcessorModalController,
                controllerAs: "ctrl",
                resolve: {
                    "subDataProcessorId": [() => subDataProcessorId],
                    "dataProcessingRegistration": [() => dataProcessingRegistration],
                    "dataProcessingRegistrationOptions": [() => dataProcessingRegistrationOptions]
                },
                backdrop: "static", //Make sure accidental click outside the modal does not close the modal
            });
        }
    }

    export class SubDataProcessorModalController {
        static $inject: Array<string> = [
            "subDataProcessorId",
            "dataProcessingRegistration",
            "dataProcessingRegistrationOptions",
            "$uibModalInstance",
            "select2LoadingService",
            "dataProcessingRegistrationService",
        ];
        
        subDataProcessorsConfig: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.DataProcessing.IDataProcessorDTO>;
        subDataProcessorTransferToThirdCountriesConfig: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YesNoUndecidedOption>;
        subDataProcessorBasisForTransferConfig: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>;
        subDataProcessorInsecureThirdCountriesConfig: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>;

        title: string;
        subDataProcessorFieldName: string;
        isEdit: boolean;
        subDataProcessorName: string;
        helpTextKey: string;

        viewModel: Models.ViewModel.GDPR.SubDataProcessorViewModel;

        readonly yesValue = Models.Api.Shared.YesNoUndecidedOption.Yes;

        private isBusy = false;

        constructor(
            private readonly subDataProcessorId: number | null,
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO,
            private readonly dataProcessingRegistrationOptions: Models.DataProcessing.IDataProcessingRegistrationOptions,
            private readonly $uibModalInstance,
            private readonly select2LoadingService: Services.ISelect2LoadingService,
            private readonly dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService) {
        }

        $onInit() {

            this.isEdit = this.subDataProcessorId != null;
            const subDataProcessor = this.isEdit ? _.find(this.dataProcessingRegistration.subDataProcessors, { id: this.subDataProcessorId }) : null;

            this.subDataProcessorName = this.isEdit ? subDataProcessor.name : "";
            this.helpTextKey = this.isEdit ? "edit" : "create";

            const titleSuffix = "underdatabehandler";
            const subDataFieldNamePrefix = "Underdatabehandler";
            this.subDataProcessorFieldName = this.isEdit ? `${subDataFieldNamePrefix} *` : subDataFieldNamePrefix;
            this.title = this.isEdit ? `Rediger ${titleSuffix}` : `Opret ${titleSuffix}`;

            this.viewModel = new Models.ViewModel.GDPR.SubDataProcessorViewModel(this.subDataProcessorId,
                subDataProcessor?.cvrNumber,
                subDataProcessor?.basisForTransfer?.id,
                subDataProcessor?.transferToInsecureThirdCountries,
                subDataProcessor?.insecureCountry?.id);

            this.subDataProcessorsConfig = this.bindSubDataProcessor();
            this.subDataProcessorBasisForTransferConfig = this.bindBasisForTransfer(subDataProcessor);
            this.subDataProcessorTransferToThirdCountriesConfig = this.bindTransferToThirdCountriesOptions(subDataProcessor);
            this.subDataProcessorInsecureThirdCountriesConfig = this.bindInsecureThirdCountries(subDataProcessor);
        }

        save(): void {
            if (this.isBusy || this.isInvalid())
                return;
            this.isBusy = true;

            const payload = this.viewModel.prepareRequestPayload();
            if (this.isEdit) {
                this.dataProcessingRegistrationService
                    .updateSubDataProcessor(this.dataProcessingRegistration.id, payload)
                    .then(() => {
                            this.isBusy = false;
                            this.close(true);
                        },
                        () => this.isBusy = false);
            } else {
                this.dataProcessingRegistrationService
                    .assignSubDataProcessor(this.dataProcessingRegistration.id, payload)
                    .then(() => {
                        this.isBusy = false;
                        this.close(true);
                    }, () => this.isBusy = false);
            }
        }

        close(isSubProcessorChanged = false): void {
            if (this.isBusy)
                return;

            this.$uibModalInstance.close(isSubProcessorChanged);
        }

        isInvalid(): boolean {
            return !this.viewModel.subDataProcessorId;
        }

        private bindBasisForTransfer(subDataProcessor: Models.DataProcessing.IDataProcessorDTO | null): Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO> {
            return Helpers.Select2MappingHelper.createNewNamedEntityWithDescriptionAndExpirationStatusDtoViewModel(subDataProcessor?.basisForTransfer,
                this.dataProcessingRegistrationOptions.basisForTransferOptions,
                (newElement) => this.viewModel.updateBasisForTransfer(newElement),
                this.select2LoadingService,
                null,
                false);
        }

        private bindTransferToThirdCountriesOptions(subDataProcessor: Models.DataProcessing.IDataProcessorDTO | null): Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YesNoUndecidedOption> {
            const thirdCountriesOptions = new Models.ViewModel.Shared.YesNoUndecidedOptions();

            let selectedOption = null;
            if (subDataProcessor) {
                selectedOption = thirdCountriesOptions.getById(subDataProcessor.transferToInsecureThirdCountries);
            }

            return {
                selectedElement: selectedOption,
                select2Config: this.select2LoadingService.select2LocalDataNoSearch(() => thirdCountriesOptions.options, false),
                elementSelected: (newElement) => {

                    this.viewModel.updateTransferToInsecureThirdCountry(newElement);

                    if (this.viewModel.transferToInsecureThirdCountryId !== Models.Api.Shared.YesNoUndecidedOption.Yes) {
                        this.subDataProcessorInsecureThirdCountriesConfig.selectedElement = null;
                    }
                }
            };
        }

        private bindInsecureThirdCountries(subDataProcessor: Models.DataProcessing.IDataProcessorDTO | null): Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO> {
            return Helpers.Select2MappingHelper.createNewNamedEntityWithDescriptionAndExpirationStatusDtoViewModel(subDataProcessor?.insecureCountry,
                this.dataProcessingRegistrationOptions.thirdCountryOptions,
                (newElement) => this.viewModel.updateInsecureThirdCountry(newElement),
                this.select2LoadingService,
                null,
                false);
        }
            
        private bindSubDataProcessor(): Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.DataProcessing.IDataProcessorDTO> {

            const pageSize = 100;
            return {
                selectedElement: null,
                select2Config: this.select2LoadingService.loadSelect2WithDataSource(
                    (query) => this
                        .dataProcessingRegistrationService
                        .getApplicableSubDataProcessors(this.dataProcessingRegistration.id, query, pageSize)
                        .then(results => Helpers.Select2MappingHelper.mapDataProcessingSearchResults(results)),
                    true,
                    Helpers.Select2OptionsFormatHelper.formatOrganizationWithOptionalObjectContext),
                elementSelected: (newElement) => {
                    this.viewModel.updateSubDataProcessor(newElement);
                }
            };
        }
    }

    app.service("subDataProcessorDialogFactory", SubDataProcessorDialogFactory);
}