module Kitos.Models.ItSystem {
    export interface IDataType extends IEntity {
        Name: string;
        IsActive: boolean;
        Note: string;
        /** The DataRows that is using this data type */
        References: Array<IDataRow>;
    }
}
