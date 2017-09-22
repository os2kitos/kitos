module Kitos.Models {
    export interface IOptionEntity extends IEntity {
        Name: string;
        IsObligatory: boolean;
        IsLocallyAvaliable: boolean;
        //IsSuggestion: boolean;
        HasWriteAccess: boolean;
        Description: string;
        IsEnabled: boolean;
    }
}