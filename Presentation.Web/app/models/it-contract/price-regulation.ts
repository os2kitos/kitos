module Kitos.Models.ItContract {
    export interface IPriceRegulation extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        References: Array<IItContract>;
    }
}
