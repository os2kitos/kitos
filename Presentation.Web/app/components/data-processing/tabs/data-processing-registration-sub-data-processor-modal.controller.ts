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
            "$state",
            "$uibModalInstance",
            "dataProcessingRegistration",
            "dataProcessingRegistrationOptions",
            "select2LoadingService",
            "dataProcessingRegistrationService",
            "$stateParams"
        ];
        
        subDataProcessorsConfig: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.DataProcessing.IDataProcessorDTO>;
        subDataProcessorTransferToThirdCountriesConfig: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YesNoUndecidedOption>;
        subDataProcessorBasisForTransferConfig: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>;
        subDataProcessorInsecureThirdCountriesConfig: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>;

        title: string;
        subDataProcessorFieldName: string;
        isEdit: boolean;
        subDataProcessorName: string;

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
            
            const subDataProcessor = this.getSubDataProcessor();

            this.isEdit = !!subDataProcessor;
            this.subDataProcessorName = this.isEdit ? subDataProcessor.name : "";

            const titleSuffix = "underdatabehandler";
            this.subDataProcessorFieldName = "Underdatabehandler";
            this.title = subDataProcessor ? `Rediger ${titleSuffix}` : `Opret ${titleSuffix}`;

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
            if (this.isBusy)
                return;
            this.isBusy = true;

            const payload = this.viewModel.prepareRequestPayload();
            if (this.subDataProcessorId) {
                this.dataProcessingRegistrationService
                    .updateSubDataProcessor(this.dataProcessingRegistration.id, payload)
                    .then(() => {
                            this.isBusy = false;
                            this.cancel(true);
                        },
                        () => this.isBusy = false);

                return;
            }

            this.dataProcessingRegistrationService.assignSubDataProcessor(this.dataProcessingRegistration.id, payload)
                .then(() => {
                    this.isBusy = false;
                    this.cancel(true);
                }, () => this.isBusy = false);
        }

        cancel(reload = false): void {
            if (this.isBusy)
                return;

            this.close();
            this.popState(reload);
        }

        isInvalid(): boolean {
            return !this.viewModel.subDataProcessorId;
        }

        private close() {
            this.$uibModalInstance.close();
        }

        private popState(reload = false) {
           /* const popped = this.$state.go("^");
            if (reload) {
                return popped.then(() => this.$state.reload());
            }
            return popped;*/
        }

        private bindBasisForTransfer(subDataProcessor: Models.DataProcessing.IDataProcessorDTO | null): Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO> {
            return Helpers.Select2MappingHelper.createNewNamedEntityWithDescriptionAndExpirationStatusDtoViewModel(subDataProcessor?.basisForTransfer,
                this.dataProcessingRegistrationOptions.basisForTransferOptions,
                (newElement) => this.viewModel.updateBasisForTransfer(newElement),
                this.select2LoadingService,
                null,
                false); //We only allow selection of non-expired and this object is based on the available objects
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
                    true),
                elementSelected: (newElement) => {
                    this.viewModel.updateSubDataProcessor(newElement);
                }
            };
        }

        private getSubDataProcessor(): Models.DataProcessing.IDataProcessorDTO | null {
            return _.find(this.dataProcessingRegistration.subDataProcessors, { id: this.subDataProcessorId });
        }
    }

    /*angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.edit-registration.main.sub-data-processor", {
                url: "/sub-data-processor-modal",
                params: {
                    subDataProcessorId: null
                },
                onEnter: [
                    "$state", "$uibModal",
                    ($state, $uibModal) => {
                        $uibModal.open({
                            templateUrl: "app/components/data-processing/tabs/data-processing-registration-sub-data-processor-modal.view.html",
                            windowClass: "modal fade in",
                            resolve: {
                                dataProcessingRegistration: [
                                    "dataProcessingRegistrationService", "$stateParams", (dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService, $stateParams) => dataProcessingRegistrationService.get($stateParams.id)
                                ],
                                dataProcessingRegistrationOptions: [
                                    "dataProcessingRegistrationService", "userService", (dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService,
                                        userService: Services.IUserService) => {
                                        return userService.getUser().then(user => dataProcessingRegistrationService.getApplicableDataProcessingRegistrationOptions(user.currentOrganizationId));
                                    }
                                ]
                            },
                            controller: SubDataProcessorModalController,
                            controllerAs: "ctrl",
                        });
                    }
                ]
            });
        }]);*/
    app.service("subDataProcessorDialogFactory", SubDataProcessorDialogFactory);
}