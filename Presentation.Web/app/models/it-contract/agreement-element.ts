module Kitos.Models.ItContract {
    export interface IAgreementElement extends IEntity {
        Name: string;
        IsActive: boolean;
        Note: string;
        References: Array<IItContract>;
    }
}
