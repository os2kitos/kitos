module Kitos.Models.ItSystem {
    export interface IInterface extends IEntity {
        Name: string;
        IsActive: boolean;
        Note: string;
        References: Array<IItInterface>;
    }
}
