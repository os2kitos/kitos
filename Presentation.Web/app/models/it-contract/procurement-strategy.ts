module Kitos.Models.ItContract {
    /** It contract procurment strategy option. (udbudsstrategi) */
    export interface IProcurementStrategy extends IEntity {
        Name: string;
        IsActive: boolean;
        Note: string;
        References: Array<IItContract>;
    }
}
