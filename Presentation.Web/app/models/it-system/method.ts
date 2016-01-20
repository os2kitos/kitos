module Kitos.Models.ItSystem {
    export interface IMethod extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        References: Array<IItInterface>;
    }
}
