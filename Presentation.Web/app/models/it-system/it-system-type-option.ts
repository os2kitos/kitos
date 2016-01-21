module Kitos.Models.ItSystem {
    export interface IItSystemTypeOption {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        References: Array<IItSystem>;
    }
}
