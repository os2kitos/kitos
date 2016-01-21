module Kitos.Models.ItSystem {
    export interface IInterface extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        References: Array<IItInterface>;
    }
}
