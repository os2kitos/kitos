module Kitos.Shared.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                pageName: "@",
                hasWriteAccess: "=",
                viewModel: "<",
            },
            controller: MainContractSectionController,
            controllerAs: "ctrl",
            templateUrl: `app/shared/main-contract/main-contract-section.view.html`
        };
    }

    export interface IMainContractSectionViewModel {
        isActive: boolean;
        options: Models.ViewModel.Generic.Select2OptionViewModel<null>[];
        postMethod: (contractId: number) => ng.IPromise<void>;
        deleteMethod: (contractId: number) => ng.IPromise<void>;
        stateReloadMethod: () => ng.IPromise<void>;
        contractId: number;
    }
    
    interface IMainContractSectionController extends ng.IComponentController {
        pageName: string;
        hasWriteAccess: boolean;
        viewModel: IMainContractSectionViewModel;
    }

    class MainContractSectionController implements IMainContractSectionController {
        pageName: string | null = null;
        hasWriteAccess: boolean | null = null;
        viewModel: IMainContractSectionViewModel | null = null;
        currentContract: {id: number, text: string};

        $onInit() {
            if (this.pageName === null) {
                console.error("Missing pageName parameter for MainContractSectionController");
                return;
            }
            if (this.hasWriteAccess === null) {
                console.error("Missing hasWriteAccess parameter for MainContractSectionController");
                return;
            }
            if (this.viewModel === null) {
                console.error("Missing viewModel parameter for MainContractSectionController");
                return;
            }

            this.currentContract = { id: this.viewModel.contractId, text: "" }
        }

        saveContract(selectedContract: number) {
            if (this.currentContract.id === selectedContract){
                return;
            }

            if (selectedContract) {
                this.viewModel.postMethod(selectedContract)
                    .then(() => this.viewModel.stateReloadMethod())
                    .then(() => this.currentContract.id = selectedContract,
                        error => console.log(error));
            } else {
                this.viewModel.deleteMethod(selectedContract)
                    .then(() => this.viewModel.stateReloadMethod())
                    .then(() => this.currentContract.id = selectedContract,
                        error => console.log(error));
            }
        }
    }
    angular.module("app")
        .component("mainContractSection", setupComponent());
}