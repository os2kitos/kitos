module Kitos.Models {
    export interface IOptionEntity extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
    }
}