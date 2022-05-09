module Kitos.Services.UICustomization {

    export interface IUICustomizationService {
        /**
         * Loads the currently active UI customization configuration based on the config blueprint as well as any local adjustments
         * @param module
         */
        loadActiveConfiguration(module: Models.UICustomization.CustomizableKitosModule): angular.IPromise<Models.UICustomization.ICustomizedModuleUI>;
        /**
         * Saves the active configuration for the current organization
         * @param config
         */
        saveActiveConfiguration(config: Models.UICustomization.ICustomizedModuleUI): angular.IPromise<Models.UICustomization.ICustomizedModuleUI>;
    }


    class UICustomizationService implements IUICustomizationService {

        static $inject = ["genericApiWrapper", "userService"];

        constructor(private readonly genericApiWrapper: Services.Generic.ApiWrapper, private readonly userService: Services.IUserService) { }

        loadActiveConfiguration(module: Models.UICustomization.CustomizableKitosModule): angular.
            //TODO: Load the current organization using the userService before fetching from the backend!

            IPromise<Models.UICustomization.ICustomizedModuleUI> {
            throw new Error("Not implemented");
        }

        saveActiveConfiguration(config: Models.UICustomization.ICustomizedModuleUI): angular.
            //TODO: Load the current organization using the userService before posting to the backend!

            IPromise<Models.UICustomization.ICustomizedModuleUI> {
            throw new Error("Not implemented");
        }
    }

    app.service("uiCustomizationService", UICustomizationService);
}