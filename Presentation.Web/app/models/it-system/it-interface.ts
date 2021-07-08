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
        /** Names of organizations that uses the interface as CSV */
        UsedByOrganizationNames: string[];
    }
}
