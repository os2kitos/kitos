module Kitos.Models {
    export interface IOptionEntity extends IEntity {
        Name: string;
        IsObligatory: boolean;
        IsLocallyAvailable: boolean;
        HasWriteAccess: boolean;
        Description: string;
        IsEnabled: boolean;
    }
}