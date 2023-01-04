module Kitos.DataProcessing.Registration.Edit.SubDataProcessor {
    "use strict";

    export class SubDataProcessorModalController {
        static $inject: Array<string> = [
            "user",
            "$scope",
            "notify",
            "$state",
            "$uibModalInstance",
            "dataProcessingRegistration",
            "dataProcessingRegistrationOptions",
            "select2LoadingService",
            "dataProcessingRegistrationService",
            "selectedSubDataProcessorId"
        ];
        
        subDataProcessorsConfig: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.DataProcessing.IDataProcessorDTO>;
        subDataProcessorTransferToThirdCountriesConfig: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YesNoUndecidedOption>;
        subDataProcessorBasisForTransferConfig: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>;
        subDataProcessorInsecureThirdCountriesConfig: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>;

        yesNoUndecidedValues = Models.Api.Shared.YesNoUndecidedOption;

        viewModel = new Models.ViewModel.GDPR.SubDataProcessorViewModel();
        
        constructor(
            private readonly user: Services.IUser,
            private readonly $scope,
            private readonly notify,
            private readonly $state,
            private readonly $uibModalInstance,
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO,
            private readonly dataProcessingRegistrationOptions: Models.DataProcessing.IDataProcessingRegistrationOptions,
            private readonly select2LoadingService: Services.ISelect2LoadingService,
            private readonly dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService,
            private readonly selectedSubDataProcessorId: number) {
        }

        $onInit() {

            const test = this.selectedSubDataProcessorId;
            this.subDataProcessorsConfig = this.bindSingleSelectSubDataProcessors();
            this.subDataProcessorTransferToThirdCountriesConfig = this.bindSubDataProcessorTransferToThirdCountriesOptions();
            this.subDataProcessorBasisForTransferConfig = this.bindSubDataProcessorBasisForTransfer(this.viewModel);
            this.subDataProcessorInsecureThirdCountriesConfig = this.bindSubDataProcessorInsecureThirdCountriesOptions();
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
        }

        cancel(): void {
            this.close();
            this.popState();
        }


        private bindSubDataProcessorTransferToThirdCountriesOptions(subDprId: number = null): Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YesNoUndecidedOption> {
            const thirdCountriesOptions = new Models.ViewModel.Shared.YesNoUndecidedOptions();

            let selectedOption = null;
            if (subDprId) {
                const selectedSubDataProcessor = _.find(this.dataProcessingRegistration.subDataProcessors, { id: subDprId }) as Models.DataProcessing.IDataProcessorDTO;
                selectedOption = thirdCountriesOptions.getById(selectedSubDataProcessor.transferToInsecureThirdCountries);
            }
            return {
                selectedElement: selectedOption,
                select2Config: this.select2LoadingService.select2LocalDataNoSearch(() => thirdCountriesOptions.options, false),
                elementSelected: (newElement) => {
                    if (!!newElement) {
                        this.viewModel.transferToInsecureThirdCountryId = newElement.id;
                    }
                }
            };
        }

        private bindSubDataProcessorBasisForTransfer(vm: Models.ViewModel.GDPR.SubDataProcessorViewModel, subDprId: number = null): Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO> {
            const optionMap = Helpers.Select2MappingHelper.mapNamedEntityWithDescriptionAndExpirationStatusDtoArrayToOptionMap(this.dataProcessingRegistrationOptions.basisForTransferOptions);

            let existingChoice = null;
            if (subDprId) {
                const selectedSubDataProcessor = _.find(this.dataProcessingRegistration.subDataProcessors, { id: subDprId }) as Models.DataProcessing.IDataProcessorDTO;
                //If selected state is expired, add it for presentation reasons
                existingChoice = selectedSubDataProcessor.basisForTransfer;
                if (existingChoice && !optionMap[existingChoice.id]) {
                    optionMap[existingChoice.id] = {
                        text: `${existingChoice.name} (udgået)`,
                        id: existingChoice.id,
                        disabled: true,
                        optionalObjectContext: existingChoice
                    }
                }
            }

            const options = this.dataProcessingRegistrationOptions.basisForTransferOptions.map(option => optionMap[option.id]);

            return {
                selectedElement: existingChoice && optionMap[existingChoice.id],
                select2Config: this.select2LoadingService.select2LocalDataNoSearch(() => options, true),
                elementSelected: (newElement) => {
                    if (!!newElement) {
                        vm.basisForTransferId = newElement.id;
                    }
                }
            };
        }

        private bindSubDataProcessorInsecureThirdCountriesOptions(subDprId: number = null): Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO> {
            const optionMap = Helpers.Select2MappingHelper.mapNamedEntityWithDescriptionAndExpirationStatusDtoArrayToOptionMap(this.dataProcessingRegistrationOptions.thirdCountryOptions);

            let selectedOption = null;
            if (subDprId) {
                const selectedSubDataProcessor = _.find(this.dataProcessingRegistration.subDataProcessors, { id: subDprId }) as Models.DataProcessing.IDataProcessorDTO;
                selectedOption = _.find(optionMap, selectedSubDataProcessor.transferToInsecureThirdCountries);
            }

            const options = this.dataProcessingRegistrationOptions.thirdCountryOptions.map(option => optionMap[option.id]);

            return {
                selectedElement: selectedOption,
                select2Config: this.select2LoadingService.select2LocalDataNoSearch(() => options, false),
                elementSelected: (newElement) => {
                    if (!!newElement) {
                        this.viewModel.insecureCountryId = newElement.id;
                    }
                }
            };
        }

        private bindSingleSelectSubDataProcessors(subDprId: number = null): Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.DataProcessing.IDataProcessorDTO> {

            let selectedOption = null;
            if (subDprId) {
                selectedOption = _.find(this.dataProcessingRegistration.subDataProcessors, { id: subDprId }) as Models.DataProcessing.IDataProcessorDTO;
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
                    if (!!newElement) {
                        this.viewModel.subDataProcessorId = newElement.id;
                    }
                }
            };
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.edit-registration.main.sub-data-processor", {
                url: "/sub-data-processor-modal",
                onEnter: [
                    "$state", "$uibModal",
                    ($state, $uibModal) => {
                        $uibModal.open({
                            templateUrl: "app/components/data-processing/tabs/data-processing-registration-sub-data-processor-modal.view.html",
                            windowClass: "modal fade in",
                            resolve: {
                                user: ["userService", (userService: Services.IUserService) => userService.getUser()
                                ],
                                dataProcessingRegistration: [
                                    "dataProcessingRegistrationService", "$stateParams", (dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService, $stateParams) => dataProcessingRegistrationService.get($stateParams.id)
                                ],
                                dataProcessingRegistrationOptions: [
                                    "dataProcessingRegistrationService"/*, "user"*/, (dataProcessingRegistrationService:
                                        Services.DataProcessing.IDataProcessingRegistrationService/*,
                                        user*/) => {
                                        return dataProcessingRegistrationService.getApplicableDataProcessingRegistrationOptions(1);
                                    }
                                ],
                                selectedSubDataProcessorId: ["$stateParams", ($stateParams) => {
                                    const test = $stateParams.subDataProcessorId;
                                    return $stateParams.subDataProcessorId;
                                }]
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