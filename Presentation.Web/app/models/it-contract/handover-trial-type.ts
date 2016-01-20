module Kitos.Models.ItContract {
    export interface IHandoverTrialType extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        References: Array<IHandoverTrial>;
    }
}
