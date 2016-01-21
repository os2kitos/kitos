module Kitos.Models {
    export interface IAdminRole extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        References: Array<IAdminRight>;
        HasReadAccess: boolean;
        HasWriteAccess: boolean;
    }
}
