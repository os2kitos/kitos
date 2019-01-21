module Kitos.Models.ItSystem {
    /** Dropdown option for ItSystem, whether it has been archived or not. */
    export interface IItSystemCategories extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        References: Array<ItSystemUsage.IItSystemUsage>;
    }
}
