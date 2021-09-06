module Kitos.Models.ItContract {
    export interface IPaymentFrequency extends IEntity {
        Name: string;
        IsActive: boolean;
        Note: string;
        References: Array<IItContract>;
    }
}
