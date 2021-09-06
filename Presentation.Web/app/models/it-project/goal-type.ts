module Kitos.Models.ItProject {
    export interface IGoalType extends IEntity {
        Name: string;
        IsActive: boolean;
        Note: string;
        References: Array<IGoal>;
    }
}
