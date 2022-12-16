module Kitos.Shared.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                pageName: "@",
                hasWriteAccess: "=",
                contractId: "=",
                save: "&",
                options: "<"
            },
            controller: MainContractSectionController,
            controllerAs: "ctrl",
            templateUrl: `app/shared/main-contract/main-contract-section.view.html`
        };
    }

    //TODO: Create a model containing data (currentId, isActive etc.) and the save method

    interface IMainContractSectionController extends ng.IComponentController {
        pageName: string;
        hasWriteAccess: boolean;
        options: Models.ViewModel.Generic.Select2OptionViewModel<null>[];
        save: (id: number) => void;
        contractId: number;
    }

    class MainContractSectionController implements IMainContractSectionController {
        pageName: string | null = null;
        hasWriteAccess: boolean | null = null;
        options: Models.ViewModel.Generic.Select2OptionViewModel<null>[];
        save: (id: number) => void | null = null;
        contractId: number;


        $onInit() {
            if (this.pageName === null) {
                console.error("Missing pageName parameter for MainContractSectionController");
                return;
            }
            if (this.hasWriteAccess === null) {
                console.error("Missing hasWriteAccess parameter for MainContractSectionController");
                return;
            }
            if (this.options === null) {
                console.error("Missing options parameter for MainContractSectionController");
                return;
            }
            if (this.save === null) {
                console.error("Missing save (method) parameter for MainContractSectionController");
                return;
            }
        }
    }
    angular.module("app")
        .component("mainContractSection", setupComponent());
}