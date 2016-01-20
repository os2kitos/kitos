module Kitos.Models.ItProject {
    export interface IGoalType extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        References: Array<IGoal>;
    }
}
