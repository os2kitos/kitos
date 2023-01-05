module Kitos.DataProcessing.Registration.Edit.SubDataProcessor {
    "use strict";

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

        readonly yesValue = Models.Api.Shared.YesNoUndecidedOption.Yes;
        title: string;

        viewModel: Models.ViewModel.GDPR.SubDataProcessorViewModel;

        private subDataProcessorId: number | null;
        private isBusy = false;

        constructor(
            private readonly $state,
            private readonly $uibModalInstance,
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO,
            private readonly dataProcessingRegistrationOptions: Models.DataProcessing.IDataProcessingRegistrationOptions,
            private readonly select2LoadingService: Services.ISelect2LoadingService,
            private readonly dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService,
            private readonly $stateParams) {
        }

        $onInit() {
            
            this.subDataProcessorId = this.$stateParams.subDataProcessorId;
            const subDataProcessor = this.getSubDataProcessor();

            const titleSuffix = "underdatabehandler";
            this.title = subDataProcessor ? `Rediger ${titleSuffix}` : `Opret ${titleSuffix}`;

            this.viewModel = new Models.ViewModel.GDPR.SubDataProcessorViewModel(this.subDataProcessorId,
                subDataProcessor?.cvrNumber,
                subDataProcessor?.basisForTransfer?.id,
                subDataProcessor?.transferToInsecureThirdCountries,
                subDataProcessor?.insecureCountry?.id);

            this.subDataProcessorsConfig = this.bindSubDataProcessor(subDataProcessor);
            this.subDataProcessorBasisForTransferConfig = this.bindBasisForTransfer(subDataProcessor);
            this.subDataProcessorTransferToThirdCountriesConfig = this.bindTransferToThirdCountriesOptions(subDataProcessor);
            this.subDataProcessorInsecureThirdCountriesConfig = this.bindInsecureThirdCountries(subDataProcessor);
        }

        private close() {
            this.$uibModalInstance.close();
        }

        private popState(reload = false) {
            const popped = this.$state.go("^");
            if (reload) {
                return popped.then(() => this.$state.reload());
            }
            return popped;
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

        private updateMapForExistingChoice<T extends Models.Generic.NamedEntity.NamedEntityDTO>(existingChoice: T, map: Models.ViewModel.Generic.Select2OptionViewModel<T>[]): T {
            if (existingChoice && !map[existingChoice.id]) {
                map[existingChoice.id] = {
                    text: existingChoice.name,
                    id: existingChoice.id,
                    optionalObjectContext: existingChoice
                }
            }

            return existingChoice;
        }

        private bindBasisForTransfer(subDataProcessor: Models.DataProcessing.IDataProcessorDTO | null): Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO> {
            const optionMap = Helpers.Select2MappingHelper.mapNamedEntityWithDescriptionAndExpirationStatusDtoArrayToOptionMap(this.dataProcessingRegistrationOptions.basisForTransferOptions);

            let existingChoice = null;
            if (subDataProcessor) {
                //If selected state is expired, add it for presentation reasons
                existingChoice = this.updateMapForExistingChoice(subDataProcessor.basisForTransfer, optionMap);
            }

            const options = this.dataProcessingRegistrationOptions.basisForTransferOptions.map(option => optionMap[option.id]);

            return {
                selectedElement: existingChoice && optionMap[existingChoice.id],
                select2Config: this.select2LoadingService.select2LocalDataNoSearch(() => options, true),
                elementSelected: (newElement) => {
                    this.viewModel.basisForTransferId = newElement?.id;
                }
            };
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
                    this.viewModel.transferToInsecureThirdCountryId = newElement?.id;
                    if (this.viewModel.transferToInsecureThirdCountryId !== Models.Api.Shared.YesNoUndecidedOption.Yes) {
                        this.viewModel.insecureCountryId = null;
                        this.subDataProcessorInsecureThirdCountriesConfig.selectedElement = null;
                    }
                }
            };
        }

        private bindInsecureThirdCountries(subDataProcessor: Models.DataProcessing.IDataProcessorDTO | null): Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO> {
            const optionMap = Helpers.Select2MappingHelper.mapNamedEntityWithDescriptionAndExpirationStatusDtoArrayToOptionMap(this.dataProcessingRegistrationOptions.thirdCountryOptions);

            let existingChoice = null;
            if (subDataProcessor) {
                //If selected state is expired, add it for presentation reasons
                existingChoice = this.updateMapForExistingChoice(subDataProcessor.insecureCountry, optionMap);
            }

            const options = this.dataProcessingRegistrationOptions.thirdCountryOptions.map(option => optionMap[option.id]);

            return {
                selectedElement: existingChoice && optionMap[existingChoice.id],
                select2Config: this.select2LoadingService.select2LocalDataNoSearch(() => options, false),
                elementSelected: (newElement) => {
                    if (this.viewModel.transferToInsecureThirdCountryId !== Models.Api.Shared.YesNoUndecidedOption.Yes)
                        return;

                    this.viewModel.insecureCountryId = newElement?.id;
                }
            };
        }

        private bindSubDataProcessor(subDataProcessor: Models.DataProcessing.IDataProcessorDTO | null): Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.DataProcessing.IDataProcessorDTO> {

            let selectedOption: Models.ViewModel.Generic.Select2OptionViewModel<Models.DataProcessing.IDataProcessorDTO> | null = null;
            if (subDataProcessor) {
                selectedOption = {
                    id: subDataProcessor.id,
                    text: subDataProcessor.name,
                    optionalObjectContext: subDataProcessor
                } as Models.ViewModel.Generic.Select2OptionViewModel<Models.DataProcessing.IDataProcessorDTO>;
            }

            const pageSize = 100;
            return {
                selectedElement: selectedOption,
                select2Config: this.select2LoadingService.loadSelect2WithDataSource(
                    (query) => this
                        .dataProcessingRegistrationService
                        .getApplicableSubDataProcessors(this.dataProcessingRegistration.id, query, pageSize)
                        .then(results => Helpers.Select2MappingHelper.mapDataProcessingSearchResults(results)),
                    true),
                elementSelected: (newElement) => {
                    this.viewModel.subDataProcessorId = newElement?.id;
                    this.viewModel.cvrNumber = newElement?.optionalObjectContext.cvrNumber;
                }
            };
        }

        private getSubDataProcessor(): Models.DataProcessing.IDataProcessorDTO {
            return _.find(this.dataProcessingRegistration.subDataProcessors, { id: this.subDataProcessorId }) as Models.DataProcessing.IDataProcessorDTO;
        }
    }

    angular
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