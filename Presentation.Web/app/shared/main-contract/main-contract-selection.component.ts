module Kitos.Shared.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                mainText: "@",
                hasWriteAccess: "=",
                viewModel: "<",
            },
            controller: MainContractSelectionController,
            controllerAs: "ctrl",
            templateUrl: "app/shared/main-contract/main-contract-selection.view.html"
        };
    }

    export interface IMainContractSelectionViewModel {
        isActive: boolean;
        selectedContractId: number;
        options: Models.ViewModel.Generic.Select2OptionViewModel<null>[];
        selectContract: (contractId: number) => ng.IPromise<void>;
        deselectContract: () => ng.IPromise<void>;
        reloadSelectedContractState: () => ng.IPromise<void>;
    }

    interface IMainContractSelectionController extends ng.IComponentController {
        mainText: string;
        hasWriteAccess: boolean;
        viewModel: IMainContractSelectionViewModel;
    }

    class MainContractSelectionController implements IMainContractSelectionController {
        mainText: string | null = null;
        hasWriteAccess: boolean | null = null;
        viewModel: IMainContractSelectionViewModel | null = null;
        contractModel: Models.ViewModel.Generic.Select2OptionViewModel<null>;
        currentContractId: number | null = null;
        select2Model: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityDTO>;

        static $inject: string[] = ["select2LoadingService", "apiUseCaseFactory"];
        constructor(
            private readonly select2LoadingService: Services.ISelect2LoadingService,
            private readonly apiUseCaseFactory: Kitos.Services.Generic.IApiUseCaseFactory
        ) { }

        $onInit() {
            if (this.mainText === null) {
                console.error("Missing mainText parameter for MainContractSelectionController");
                return;
            }
            if (this.hasWriteAccess === null) {
                console.error("Missing hasWriteAccess parameter for MainContractSelectionController");
                return;
            }
            if (this.viewModel === null) {
                console.error("Missing viewModel parameter for MainContractSelectionController");
                return;
            }

            const selectedContract = _.find(this.viewModel.options, { id: this.viewModel.selectedContractId });
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

        saveContract(selectedContractId: number | null = null) {
            if (this.currentContractId === selectedContractId) {
                return;
            }
            return this.apiUseCaseFactory.createUpdate("\"Valgt kontrakt\"", () => {
                if (selectedContractId) {
                    return this.viewModel.selectContract(selectedContractId)
                        .then(() => this.reInitialize(selectedContractId), error => console.log(error));
                } else {
                    return this.viewModel.deselectContract()
                        .then(() => this.reInitialize(selectedContractId), error => console.log(error));
                }
            }).executeAsync();
        }

        private reInitialize(id: number | null) {
            return this.viewModel.reloadSelectedContractState()
                .then(() => this.updateCurrentContractId(id));
        }

        private updateCurrentContractId(id: number | null) {
            this.currentContractId = id;
        }
    }
    angular.module("app")
        .component("mainContractSelection", setupComponent());
}