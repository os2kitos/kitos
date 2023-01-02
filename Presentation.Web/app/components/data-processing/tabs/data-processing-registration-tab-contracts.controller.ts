module Kitos.DataProcessing.Registration.Edit.Contracts {
    "use strict";

    export class EditContractsDataProcessingRegistrationController {
        static $inject: Array<string> = [
            "dataProcessingRegistration",
            "entityMapper",
            "dataProcessingRegistrationService",
            "uiState",
            "hasWriteAccess"
        ];

        mainContractViewModel: Shared.Components.IMainContractSectionViewModel;
        isMainContractVisible: boolean;

        headerName = this.dataProcessingRegistration.name;
        contracts = this.dataProcessingRegistration.associatedContracts;

        private dprId = this.dataProcessingRegistration.id;

        constructor(
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO,
            private readonly entityMapper: Services.LocalOptions.IEntityMapper,
            private readonly dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService,
            private readonly uiState: Models.UICustomization.ICustomizedModuleUI,
            public readonly hasWriteAccess: boolean) {
        }

        $onInit() {
            const blueprint = Kitos.Models.UICustomization.Configs.BluePrints.DataProcessingUiCustomizationBluePrint;
            this.isMainContractVisible = this.uiState.isBluePrintNodeAvailable(blueprint.children.itContracts.children.mainContract);

            this.bindMainContract();
        }
        
        private bindMainContract() {

            const mainContractId = this.dataProcessingRegistration.mainContractId;
            const mainContractIsActive = this.dataProcessingRegistration.isActiveAccordingToMainContract;

            const contracts = this.dataProcessingRegistration.associatedContracts;
            const mappedContracts = this.entityMapper.mapApiResponseToSelect2ViewModel(contracts);

            this.mainContractViewModel = {
                options: mappedContracts,
                selectedContractId: mainContractId,
                isActive: mainContractIsActive,
                postMethod: (id: number) => this.saveMainContract(id),
                deleteMethod: () => this.deleteMainContract(),
                stateReloadMethod: () => this.reloadContractState()
            }
        }

        private saveMainContract(id: number): ng.IPromise<void> {
            return this.dataProcessingRegistrationService.updateMainContract(this.dprId, id);
        }

        private deleteMainContract(): ng.IPromise<void>{
            return this.dataProcessingRegistrationService.removeMainContract(this.dprId);
        }

        private reloadContractState(): ng.IPromise<void> {
            return this.dataProcessingRegistrationService.get(this.dprId)
                .then((dpr) => {
                    this.dataProcessingRegistration.mainContractId = dpr.mainContractId;
                    this.dataProcessingRegistration.associatedContracts = dpr.associatedContracts;
                    this.dataProcessingRegistration.isActiveAccordingToMainContract = dpr.isActiveAccordingToMainContract;

                    this.bindMainContract();
                });
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.edit-registration.contracts", {
                url: "/contracts",
                templateUrl: "app/components/data-processing/tabs/data-processing-registration-tab-contracts.view.html",
                controller: EditContractsDataProcessingRegistrationController,
                controllerAs: "vm"
            });
        }]);
}
