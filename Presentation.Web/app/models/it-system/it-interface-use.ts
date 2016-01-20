module Kitos.Models.ItSystem {
    export interface IItInterfaceUse {
        ItInterfaceId: number;
        ItInterface: IItInterface;
        ItSystemId: number;
        ItSystem: IItSystem;
        InterfaceUsages: Array<ItSystemUsage.IInterfaceUsage>;
    }
}
