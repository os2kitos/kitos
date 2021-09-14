module Kitos.Models.ItContract {
    export interface IContractType extends IEntity {
        Name: string;
        IsActive: boolean;
        Note: string;
        References: Array<IItContract>;
    }
}
