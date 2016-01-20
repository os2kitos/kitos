module Kitos.Models.ItContract {
    export interface ITerminationDeadline extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        References: IItContract;
    }
}
