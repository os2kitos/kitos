module Kitos.Models.Api.UICustomization {
    export interface ICustomizedUINodeDTO {
        fullKey: string;
        enabled: boolean;
    }
    export interface IUIModuleCustomizationDTO {
        organizationId: number;
        module: Models.UICustomization.CustomizableKitosModule;
        nodes: Array<ICustomizedUINodeDTO>;
    }
}