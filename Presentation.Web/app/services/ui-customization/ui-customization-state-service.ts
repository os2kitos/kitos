module Kitos.Services.UICustomization {

    export type UiCustomizationStateCache = Record<Models.UICustomization.CustomizableKitosModule, Models.UICustomization.ICustomizedModuleUI | null>;

    const cache: UiCustomizationStateCache =
    {
        ItSystemUsages: null
    }

    /**
     * Service used by non-admin flows to get a copy of the latest ui customization state
     */
    export interface IUICustomizationStateService {
        getCurrentState(module: Models.UICustomization.CustomizableKitosModule): ng.IPromise<Models.UICustomization.ICustomizedModuleUI>
    }

    class UICustomizationStateService implements IUICustomizationStateService {
        static $inject = ["uiCustomizationService", "uiCustomizationStateCache", "$q"];

        constructor(private readonly uiCustomizationService: IUICustomizationService, private readonly uiCustomizationStateCache: UiCustomizationStateCache, private readonly $q: ng.IQService) { }

        getCurrentState(module: Models.UICustomization.CustomizableKitosModule): ng.IPromise<Models.UICustomization.ICustomizedModuleUI> {
            const state = this.uiCustomizationStateCache[module];
            if (!state) {
                return this.uiCustomizationService.loadActiveConfiguration(module).then(state => {
                    this.uiCustomizationStateCache[module] = state;
                    return state;
                });
            }
            return this.$q.resolve(state);
        }
    }

    app.constant("uiCustomizationStateCache", cache);
    app.constant("uiCustomizationStateService", UICustomizationStateService);
}