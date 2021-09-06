module Kitos.Models.ItContract {
    export interface IPriceRegulation extends IEntity {
        Name: string;
        IsActive: boolean;
        Note: string;
        References: Array<IItContract>;
    }
}
