module Kitos.Services.UICustomization {

    export interface IUICustomizationService {
        /**
         * Loads the currently active UI customization configuration based on the config blueprint as well as any local adjustments
         * @param module
         */
        loadActiveConfiguration(module: Models.UICustomization.CustomizableKitosModule): ng.IPromise<Models.UICustomization.ICustomizedModuleUI>;
        /**
         * Saves the active configuration for the current organization
         * @param config
         */
        saveActiveConfiguration(config: Models.UICustomization.ICustomizedModuleUI): ng.IPromise<Models.UICustomization.ICustomizedModuleUI>;
    }


    class UICustomizationService implements IUICustomizationService {

        static $inject = ["genericApiWrapper", "userService"];

        constructor(private readonly genericApiWrapper: Services.Generic.ApiWrapper, private readonly userService: Services.IUserService) { }

        private loadBluePrint(module: Models.UICustomization.CustomizableKitosModule): Models.UICustomization.Configs.ICustomizableUIModuleConfigBluePrint {

            switch (module) {
                case Models.UICustomization.CustomizableKitosModule.ItSystemUsage:
                    return Models.UICustomization.Configs.BluePrints.ItSystemUsageUiCustomizationBluePrint;
                default:
                    throw `Unknown module blueprint:${module}`;
            }
        }

        private loadPersistedPreferences(module: Models.UICustomization.CustomizableKitosModule): ng.IPromise<Models.Api.UICustomization.IUIModuleCustomizationDTO> {
            return this
                .userService
                .getUser()
                .then(user => this.genericApiWrapper.getDataFromUrl<Models.Api.UICustomization.IUIModuleCustomizationDTO>("api/v1/ui-customization/modules?organizationId=" + user.currentOrganizationId)
                    .then(
                        response => response,
                        error => {
                            if (error !== Models.Api.ApiResponseErrorCategory.NotFound) {
                                console.error("Error loading module config for module ", module, " in organization ", user.currentOrganizationId, ". Failed with:", error);
                            }
                            return <Models.Api.UICustomization.IUIModuleCustomizationDTO>{
                                module: module,
                                nodes: [],
                                organizationId: user.currentOrganizationId
                            }
                        }
                    )
                );
        }

        private buildActiveConfiguration(bluePrint: Models.UICustomization.Configs.ICustomizableUIModuleConfigBluePrint, persistedPreferences: Models.Api.UICustomization.IUIModuleCustomizationDTO): Models.UICustomization.ICustomizedModuleUI {
            throw "TODO";
        }

        loadActiveConfiguration(module: Models.UICustomization.CustomizableKitosModule): ng.IPromise<Models.UICustomization.ICustomizedModuleUI> {
            let persisted: angular.IPromise<Models.Api.UICustomization.IUIModuleCustomizationDTO>;
            let bluePrint: Models.UICustomization.Configs.ICustomizableUIModuleConfigBluePrint;

            switch (module) {
                case Models.UICustomization.CustomizableKitosModule.ItSystemUsage:
                    {
                        persisted = this.loadPersistedPreferences(module);
                        bluePrint = this.loadBluePrint(module);
                        break;
                    }
                default:
                    throw `Unknown module:${module}`;
            }
            return persisted.then(config => this.buildActiveConfiguration(bluePrint, config));
        }

        saveActiveConfiguration(config: Models.UICustomization.ICustomizedModuleUI): ng.IPromise<Models.UICustomization.ICustomizedModuleUI> {
            throw new Error("Not implemented");
        }


    }

    app.service("uiCustomizationService", UICustomizationService);
}