module Kitos.Shared.Components.Progress {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                showText: "@",
                customText: "@"
            },
            controller: ProgressSpinnerComponentController,
            controllerAs: "ctrl",
            templateUrl: `app/shared/progress-spinner/progress-spinner.view.html`
        };
    }

    interface IProgressSpinnerComponentController extends ng.IComponentController {
        showText: boolean
        customText: string | null | undefined;

    }

    class ProgressSpinnerComponentController implements IProgressSpinnerComponentController {
        showText: boolean
        customText: string | undefined;

        activeText: string | null = null;
        $onInit() {
            console.log("showtext:", this.showText, "customText", this.customText);
            if (this.showText) {
                this.activeText = this.customText ?? "Indlæser indhold...";
            }
        }
    }
    angular.module("app")
        .component("progressSpinner", setupComponent());
}