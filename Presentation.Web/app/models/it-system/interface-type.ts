module Kitos.Models.ItSystem {
    export interface IInterfaceType extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        References: Array<IItInterface>;
    }
}
