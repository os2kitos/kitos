module Kitos.Models.ItSystem {
    export interface ITsa extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        References: Array<IItInterface>;
    }
}
