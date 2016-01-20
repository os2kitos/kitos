module Kitos.Models.ItSystemUsage {
    export interface IInterfaceUsage {
        ItSystemUsageId: number;
        /** The system that is using the interface. */
        ItSystemUsage: IItSystemUsage;
        ItContractId: number;
        /** The contract for this interface usage. */
        ItContract: ItContract.IItContract;
        /** Local details regarding the usage of the exposed data of the interface */
        DataRowUsages: Array<IDataRowUsage>;
        InfrastructureId: number;
        /** An ItSystem marked as infrastructure for the local usage of the interface. */
        Infrastructure: IItSystemUsage;
        ItInterfaceId: number;
        ItSystemId: number;
        ItInterfaceUse: ItSystem.IItInterfaceUse;
        /** Whether local usage of the interface is wanted or not. */
        IsWishedFor: boolean;
    }
}
