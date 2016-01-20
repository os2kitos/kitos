module Kitos.Models.ItContract {
    export interface IPurchaseForm extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        References: Array<IItContract>;
    }
}
