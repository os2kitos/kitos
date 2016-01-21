module Kitos.Models.ItProject {
    /** It project handover data. */
    export interface IHandover extends IEntity {
        Description: string;
        MeetingDate: Date;
        Summary: string;
        /** Gets or sets the associated it project. */
        ItProject: IItProject;
        /** Gets or sets the users that participated in the handover. */
        Participants: Array<IUser>;
    }
}
