module Kitos.Models.ItContract {
    export interface ICriticalityType extends IEntity {
        Name: string;
        IsActive: boolean;
        Note: string;
        References: Array<IItContract>;
    }
}