module Kitos.Models.ItSystem {
    export interface IDataRow extends IEntity {
        ItInterfaceId: number;
        /** The interface which exposes the data */
        ItInterface: IItInterface;
        DataTypeId: number;
        /** The type of the data */
        DataType: Models.OData.Generic.IOptionDTO<IDataRow>;
        /** Description/name of the data */
        Data: string;
    }
}
