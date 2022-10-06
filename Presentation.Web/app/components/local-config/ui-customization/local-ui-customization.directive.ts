module Kitos.LocalAdmin.Directives {
    "use strict";

    function setupDirective(): ng.IDirective {
        return {
            scope: {
                customizedModuleId: "@"
            },
            controller: LocalUiCustomizationController,
            controllerAs: "ctrl",
            templateUrl: `app/components/local-config/ui-customization/local-ui-customization.view.html`
        };
    }

    interface IDirectiveScope {
        /**
         * The full ui configuration view model
         */
        customizationModel: Models.UICustomization.ICustomizedModuleUI | null;
        /**
         * Determines the Id of the customization module
         */
        customizedModuleId: Models.UICustomization.CustomizableKitosModule;
        /**
         * Determines if the editor is open or closed
         */
        isOpen: boolean;
        /**
         * Update function to toggle a specific setting
         * @param key
         */
        toggleSetting(key : string) : void;
    }

    class LocalUiCustomizationController implements IDirectiveScope {
        customizationModel: Models.UICustomization.ICustomizedModuleUI | null = null;
        customizedModuleId: Models.UICustomization.CustomizableKitosModule;
        isOpen: boolean;
        subtreeIsCompleteHelpText: string;

        static $inject: string[] = ["$scope", "uiCustomizationService"];

        constructor(
            $scope,
            private readonly uiCustomizationService: Services.UICustomization.IUICustomizationService) {
            this.isOpen = false;
            this.customizedModuleId = $scope.customizedModuleId;
            uiCustomizationService //Load a fresh configuration
                .loadActiveConfiguration(this.customizedModuleId)
                .then(customizationModel => {
                    this.customizationModel = customizationModel;
                });

            this.subtreeIsCompleteHelpText = Kitos.Models.UICustomization.Configs.helpTexts.subtreeIsCompleteHelpText;
        }

        toggleSetting(key : string) {
            const node = this.customizationModel.locateNode(key);

            if (node.editable) {
                this.customizationModel.changeAvailableState(node.key, !node.available);
                return this.uiCustomizationService.saveActiveConfiguration(this.customizationModel);
            }

            return false;
        }
    }
    angular.module("app")
        .directive("localUiCustomization", setupDirective);
}