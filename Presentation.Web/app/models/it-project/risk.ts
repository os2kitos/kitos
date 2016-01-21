module Kitos.Models.ItProject {
    export interface IRisk extends IEntity {
        /** Gets or sets the associated it project identifier. */
        ItProjectId: number;
        /** Gets or sets the associated it project. */
        ItProject: IItProject;
        Name: string;
        Action: string;
        Probability: number;
        Consequence: number;
        ResponsibleUserId: number;
        ResponsibleUser: IUser;
    }
}
