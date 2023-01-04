module Kitos.Shared.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                mainText: "@",
                hasWriteAccess: "=",
                isVisible: "=",
                viewModel: "<",
            },
            controller: MainContractSectionController,
            controllerAs: "ctrl",
            templateUrl: `app/shared/main-contract/main-contract-section.view.html`
        };
    }

    export interface IMainContractSectionViewModel {
        isActive: boolean;
        selectedContractId: number;
        options: Models.ViewModel.Generic.Select2OptionViewModel<null>[];
        postMethod: (contractId: number) => ng.IPromise<void>;
        deleteMethod: () => ng.IPromise<void>;
        stateReloadMethod: () => ng.IPromise<void>;
    }
    
    interface IMainContractSectionController extends ng.IComponentController {
        mainText: string;
        hasWriteAccess: boolean;
        isVisible: boolean;
        viewModel: IMainContractSectionViewModel;
    }

    class MainContractSectionController implements IMainContractSectionController {
        mainText: string | null = null;
        hasWriteAccess: boolean | null = null;
        isVisible: boolean | null = null;
        viewModel: IMainContractSectionViewModel | null = null;
        contractModel: Models.ViewModel.Generic.Select2OptionViewModel<null>;
        currentContractId: number = null;
        select2Model: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityDTO>;

        static $inject: string[] = ["select2LoadingService", "organizationApiService", "notify"];
        constructor(private readonly select2LoadingService: Services.ISelect2LoadingService) {}

        $onInit() {
            if (this.mainText === null) {
                console.error("Missing mainText parameter for MainContractSectionController");
                return;
            }
            if (this.hasWriteAccess === null) {
                console.error("Missing hasWriteAccess parameter for MainContractSectionController");
                return;
            }
            if (this.isVisible === null) {
                console.error("Missing isVisible parameter for MainContractSectionController");
                return;
            }
            if (this.viewModel === null) {
                console.error("Missing viewModel parameter for MainContractSectionController");
                return;
            }

            const selectedContract = _.find(this.viewModel.options, { id: this.viewModel.selectedContractId }) as Models.ViewModel.Generic.Select2OptionViewModel<null>;
            if (selectedContract !== null) {
                this.contractModel = selectedContract;
                this.updateCurrentContractId(this.viewModel.selectedContractId);
            }

            this.select2Model = {
                selectedElement: this.contractModel,
                select2Config: this.select2LoadingService.select2LocalDataNoSearch(() => this.viewModel.options, true),
                elementSelected: (contract) => this.saveContract(contract?.id)
            };
        }

        saveContract(selectedContractId: number = null) {
            if (this.currentContractId === selectedContractId){
                return;
            }

            if (selectedContractId) {
                this.viewModel.postMethod(selectedContractId)
                    .then(() => this.viewModel.stateReloadMethod())
                    .then(() => this.updateCurrentContractId(selectedContractId),
                        error => console.log(error));
            } else {
                this.viewModel.deleteMethod()
                    .then(() => this.viewModel.stateReloadMethod())
                    .then(() => this.updateCurrentContractId(selectedContractId),
                        error => console.log(error));
            }
        }

        private updateCurrentContractId(id: number) {
            this.currentContractId = id;
        }
    }
    angular.module("app")
        .component("mainContractSection", setupComponent());
}