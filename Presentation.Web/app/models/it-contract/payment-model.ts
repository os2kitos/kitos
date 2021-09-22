module Kitos.Models.ItContract {
    export interface IPaymentModel extends IEntity {
        Name: string;
        IsActive: boolean;
        Note: string;
        References: Array<IItContract>;
    }
}
