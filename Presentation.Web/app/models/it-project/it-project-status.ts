module Kitos.Models.ItProject {
    export interface IItProjectStatus extends IEntity {
        /** Human readable ID ("brugervendt noegle" in OIO) */
        HumanReadableId: string;
        Name: string;
        /** Description of the state */
        Description: string;
        Note: string;
        /** Estimate for the time needed to reach this state */
        TimeEstimate: number;
        AssociatedUserId: number;
        /** User which is somehow associated with this state */
        AssociatedUser: IUser;
        AssociatedItProjectId: number;
        /** Gets or sets the associated it project. */
        AssociatedItProject: IItProject;
        /** Gets or sets the associated phase. */
        AssociatedPhaseNum: number;
    }
}
