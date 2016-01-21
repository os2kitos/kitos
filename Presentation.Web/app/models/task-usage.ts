module Kitos.Models {
    export interface ITaskUsage extends IEntity {
        TaskRefId: number;
        /** The task in use */
        TaskRef: ITaskRef;
        OrgUnitId: number;
        /** The organization unit which uses the task */
        OrgUnit: IOrganizationUnit;
        ParentId: number;
        /** If the parent  of  also has marked the ,the parent usage is accesible from here. */
        Parent: ITaskUsage;
        /** Child usages (see ) */
        Children: Array<ITaskUsage>;
        /** Whether the TaskUsage can be found on the overview */
        Starred: boolean;
        TechnologyStatus: TrafficLight;
        UsageStatus: TrafficLight;
        Comment: string;
    }
}
