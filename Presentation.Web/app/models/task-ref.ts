module Kitos.Models {
    import IItProject = Models.ItProject.IItProject;
    import IItSystem = Models.ItSystem.IItSystem;
    import IItSystemUsage = Models.ItSystemUsage.IItSystemUsage;

    export interface ITaskRef {
        /** Gets or sets the access modifier. */
        AccessModifier: AccessModifier;
        /** Global ID */
        Uuid: any;
        /** Type of task. In KLE, this is used to distinguish between tasks and task groups */
        Type: string;
        /** Human readable ID */
        TaskKey: string;
        /** Further description */
        Description: string;
        ActiveFrom: Date;
        ActiveTo: Date;
        ParentId: number;
        OwnedByOrganizationUnitId: number;
        /** The organization unit the task was created in. */
        OwnedByOrganizationUnit: IOrganizationUnit;
        Parent: ITaskRef;
        Children: Array<ITaskRef>;
        /** ItProjects which have been marked with this task */
        ItProjects: Array<IItProject>;
        /** Usages of this task */
        Usages: Array<ITaskUsage>;
        /** ItSystems which have been marked with this task */
        ItSystems: Array<IItSystem>;
        /** ItSystemUsages which have been marked with this task */
        ItSystemUsages: Array<IItSystemUsage>;
    }
}
