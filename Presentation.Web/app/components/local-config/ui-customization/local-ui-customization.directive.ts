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
        customizationModel: Models.UICustomization.ICustomizedModuleUI | null;
        customizedModuleId: Models.UICustomization.CustomizableKitosModule;
        //TODO: OnClicked! to do the changes!
        //TODO: One-way binding only to do the rendering! (on the node level but not on the root level)
    }

    class LocalUiCustomizationController implements IDirectiveScope {
        customizationModel: Models.UICustomization.ICustomizedModuleUI | null = null;
        customizedModuleId: Models.UICustomization.CustomizableKitosModule;

        static $inject: string[] = ["$scope", "uiCustomizationService", "uiCustomizationStateService"];

        constructor(
            $scope,
            private readonly uiCustomizationService: Services.UICustomization.IUICustomizationService,
            private readonly uiCustomizationStateService: Services.UICustomization.IUICustomizationStateService) {

            this.customizedModuleId = $scope.customizedModuleId;
            uiCustomizationStateService
                .getCurrentState(this.customizedModuleId)
                .then(customizationModel => {
                    this.customizationModel = customizationModel;
                });
        }
    }
    angular.module("app")
        .directive("localUiCustomization", setupDirective);
}