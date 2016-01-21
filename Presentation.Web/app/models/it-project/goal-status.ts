module Kitos.Models.ItProject {
	/** It project goal status. */
    export interface IGoalStatus extends IEntity {
        ItProject: IItProject;
        /** Traffic-light dropdown for overall status. */
        Status: TrafficLight;
        /** Date-for-status-update field */
        StatusDate: Date;
        /** Notes on collected status on project */
        StatusNote: string;
        Goals: Array<IGoal>;
    }
}
