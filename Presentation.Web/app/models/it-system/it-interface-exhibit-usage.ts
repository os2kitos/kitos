module Kitos.Models.ItSystem {
    export interface IItInterfaceExhibitUsage {
        ItSystemUsageId: number;
        /** The local usage of the system that is exposing the interface. */
        ItSystemUsage: ItSystemUsage.IItSystemUsage;
        /** The contract for this interface exposure. */
        ItContractId: number;
        ItContract: ItContract.IItContract;
        ItInterfaceExhibitId: number;
        /** The interface that is being exhibited. */
        ItInterfaceExhibit: IItInterfaceExhibit;
        /** Whether local exposure of the interface is wanted or not. */
        IsWishedFor: boolean;
    }
}
