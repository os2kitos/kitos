module Kitos.Models.ItProject {
    export interface IStakeholder extends IEntity {
        /** Gets or sets the associated it project identifier. */
        ItProjectId: number;
        /** Gets or sets the associated it project. */
        ItProject: IItProject;
        Name: string;
        Role: string;
        Downsides: string;
        Benefits: string;
        Significance: number;
        HowToHandle: string;
    }
}
