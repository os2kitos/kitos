module Kitos.Models.ItSystemUsage {
    export interface IDataRowUsage {
        ItSystemUsageId: number;
        ItSystemId: number;
        ItInterfaceId: number;
        /** The local usage of an interface, which this DataRowUsage is bound to. */
        InterfaceUsage: IInterfaceUsage;
        DataRowId: number;
        /** The DataRow that is in use. */
        DataRow: ItSystem.IDataRow;
        FrequencyId: number;
        /** How often the data of the DataRow is used */
        Frequency: IFrequency;
        /** How much the data of the DataRow is used */
        Amount: number;
        /** Details regarding total economy of the usage of DataRow */
        Economy: number;
        /** Details regarding the price of this DataRow */
        Price: number;
    }
}
