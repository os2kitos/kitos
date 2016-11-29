module Kitos.Models.ItProject {
    export interface IItProjectStatusUpdate extends IEntity {
        CombinedStatus: String;
        TimeStatus: String;
        QualityStatus: String;
        ResourcesStatus: String;
        IsCombined: Boolean;
        Created: Date;
        IsFinal: Boolean;
        Note: string;
        AssociatedItProjectId: number;
        /** Gets or sets the associated it project. */
        AssociatedItProject: IItProject;
    }
}
