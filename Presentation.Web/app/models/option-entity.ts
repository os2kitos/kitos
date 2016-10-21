module Kitos.Models {
    export interface IOptionEntity extends IEntity {
        Name: string;
        IsObligatory: boolean;
        IsActive: boolean;
        //IsSuggestion: boolean;
        HasWriteAccess: boolean;
        Description: string;
        IsDisabled: boolean;
    }
}