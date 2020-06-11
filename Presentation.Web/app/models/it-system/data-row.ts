module Kitos.Models.ItSystem {
    export interface IDataRow extends IEntity {
        ItInterfaceId: number;
        /** The interface which exposes the data */
        ItInterface: IItInterface;
        DataTypeId: number;
        /** The type of the data */
        DataType: IDataType;
        /** Description/name of the data */
        Data: string;
    }
}
