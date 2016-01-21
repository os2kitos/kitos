module Kitos.Models.ItProject {
    export interface IItProjectType extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        References: Array<IItProject>;
    }
}
