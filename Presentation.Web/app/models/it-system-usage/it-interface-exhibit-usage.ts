module Kitos.Models.ItSystemUsage {
    export interface IItInterfaceExhibitUsage {
        ItSystemUsageId: number;
        /** The local usage of the system that is exposing the interface. */
        ItSystemUsage: IItSystemUsage;
        /** The contract for this interface exposure. */
        ItContractId: number;
        ItContract: ItContract.IItContract;
        ItInterfaceExhibitId: number;
        /** The interface that is being exhibited. */
        ItInterfaceExhibit: ItSystem.IItInterfaceExhibit;
        /** Whether local exposure of the interface is wanted or not. */
        IsWishedFor: boolean;
    }
}
