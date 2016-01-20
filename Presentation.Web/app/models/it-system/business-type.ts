module Kitos.Models.ItSystem {
    /** Dropdown option for ItSystem, representing the business type of the system */
    export interface IBusinessType extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        /** The ItSystems that has been marked with this BusinessType */
        References: Array<IItSystem>;
    }
}
