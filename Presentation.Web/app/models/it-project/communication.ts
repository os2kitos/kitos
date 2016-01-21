module Kitos.Models.ItProject {
    export interface ICommunication extends IEntity {
        /** Gets or sets the target audiance. */
        TargetAudiance: string;
        /** Gets or sets the purpose. */
        Purpose: string;
        /** Gets or sets the message. */
        Message: string;
        /** Gets or sets the media. */
        Media: string;
        /** Gets or sets the due date. */
        DueDate: Date;
        /** Gets or sets the responsible user identifier. */
        ResponsibleUserId: number;
        /** Gets or sets the responsible user. */
        ResponsibleUser: IUser;
        /** Gets or sets the associated it project identifier. */
        ItProjectId: number;
        /** Gets or sets the associated it project. */
        ItProject: IItProject;
    }
}
