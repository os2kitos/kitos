module Kitos.Models.ItContract {
    /** It contract procurment strategy option. (Genanskaffelsesstrategi) */
    export interface IProcurementStrategy extends IEntity {
        Name: string;
        IsActive: boolean;
        Note: string;
        References: Array<IItContract>;
    }
}
