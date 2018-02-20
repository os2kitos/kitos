module Kitos.Models.ItSystem {
    /** Dropdown option for ItSystem, whether it has been archived or not. */
    export interface IArchiveLocation extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        /** The ItSystems that has been marked with this ArchiveType */
        References: Array<ItSystemUsage.IItSystemUsage>;
    }
}