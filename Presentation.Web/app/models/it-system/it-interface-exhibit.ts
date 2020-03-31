module Kitos.Models.ItSystem {
    export interface IItInterfaceExhibit extends IEntity {
        ItSystemId: number;
        ItSystem: IItSystem;
        ItInterface: IItInterface;
    }
}
