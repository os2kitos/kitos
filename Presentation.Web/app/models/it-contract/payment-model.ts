module Kitos.Models.ItContract {
    export interface IPaymentModel extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        References: Array<IItContract>;
    }
}
