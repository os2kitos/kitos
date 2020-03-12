module Kitos.Models.ItSystem {
    export interface IItInterface extends IItSystemBase {
        /** Gets or sets the version. */
        Version: string;
        /** Gets or sets the user defined interface identifier. */
        ItInterfaceId: string;
        InterfaceId: number;
        /** Gets or sets the interface option.Provides details about an it system of type interface. */
        Interface: IInterface;
        DataRows: Array<IDataRow>;
        Note: string;
        Disabled: boolean;
        /** Gets or sets it systems that can use this instance. */
        CanBeUsedBy: Array<IItInterfaceUse>;
        /** Gets or sets it system that exhibits this interface instance. */
        ExhibitedBy: IItInterfaceExhibit;
        /** Gets or sets local usages of the system, in case the system is an interface. */
        InterfaceLocalUsages: Array<ItSystemUsage.IInterfaceUsage>;
        /** Gets or sets local exposure of the system, in case the system is an interface. */
        InterfaceLocalExposure: Array<IItInterfaceExhibitUsage>;
    }
}
