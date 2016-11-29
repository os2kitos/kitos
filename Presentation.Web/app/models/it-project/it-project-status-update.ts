module Kitos.Models.ItProject {
    export interface IItProjectStatusUpdate extends IEntity {
        CombinedStatus: TrafficLight;
        TimeStatus: TrafficLight;
        QualityStatus: TrafficLight;
        ResourcesStatus: TrafficLight;
        IsCombined: Boolean;
        Created: Date;
        IsFinal: Boolean;
        Note: string;
        AssociatedItProjectId: number;
        /** Gets or sets the associated it project. */
        AssociatedItProject: IItProject;
    }
}
