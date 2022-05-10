module Kitos.Models.Api.UICustomization {
    export interface ICustomizedUINodeDTO {
        key: string;
        enabled: boolean;
    }
    export interface IUIModuleCustomizationDTO {
        nodes: Array<ICustomizedUINodeDTO>;
    }
}