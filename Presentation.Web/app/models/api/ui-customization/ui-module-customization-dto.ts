module Kitos.Models.Api.UICustomization {
    export interface ICustomizedUINodeDTO {
        fullKey: string;
        enabled: boolean;
    }
    export interface IUIModuleCustomizationDTO {
        nodes: Array<ICustomizedUINodeDTO>;
    }
}