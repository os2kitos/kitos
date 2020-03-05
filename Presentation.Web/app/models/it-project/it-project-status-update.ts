module Kitos.Models.ItProject {
    export interface IItProjectStatusUpdate extends IEntity {
        CombinedStatus: string;
        TimeStatus: string;
        QualityStatus: string;
        ResourcesStatus: string;
        IsCombined: boolean;
        Created: Date;
        IsFinal: boolean;
        Note: string;
        AssociatedItProjectId: number;
        /** Gets or sets the associated it project. */
        AssociatedItProject: IItProject;
    }
}
