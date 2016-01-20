module Kitos.Models.ItProject {
	/** It project goal. */
    export interface IGoal extends IEntity {
        /** Human readable identifier. */
        HumanReadableId: string;
        Name: string;
        Description: string;
        Note: string;
        Measurable: boolean;
        Status: any;
        GoalTypeId: number;
        GoalType: IGoalType;
        GoalStatusId: number;
        GoalStatus: IGoalStatus;
        SubGoalDate1: Date;
        SubGoalDate2: Date;
        SubGoalDate3: Date;
        SubGoal1: string;
        SubGoal2: string;
        SubGoal3: string;
        SubGoalRea1: string;
        SubGoalRea2: string;
        SubGoalRea3: string;
    }
}
