module Kitos.Models.ItProject {
    export interface IItProject extends IEntity {
        /** Gets or sets the user defined it project identifier. */
        ItProjectId: string;
        Background: string;
        Note: string;
        Name: string;
        Description: string;
        AccessModifier: any;
        IsArchived: boolean;
        Esdh: string;
        Cmdb: string;
        Folder: string;
        ParentId: number;
        Parent: IItProject;
        Children: Array<IItProject>;

        ItProjectTypeId: number;
        ItProjectType: IItProjectType;

        /** Gets or sets the organization identifier in which this project was created. */
        OrganizationId: number;
        /** Gets or sets the organization in which this project was created. */
        Organization: IOrganization;
        Priority: ItProjectPriority;
        IsPriorityLocked: boolean;
        PriorityPf: ItProjectPriority;

        /** Gets or sets a value indicating whether this instance is transversal. (tværgående) */
        IsTransversal: boolean;
        IsStatusGoalVisible: boolean;
        IsStrategyVisible: boolean;
        IsRiskVisible: boolean;
        IsHierarchyVisible: boolean;
        IsEconomyVisible: boolean;
        IsStakeholderVisible: boolean;
        IsCommunicationVisible: boolean;
        IsHandoverVisible: boolean;
        Handover: IHandover;
        Communications: Array<ICommunication>;
        TaskRefs: Array<ITaskRef>;
        Risks: Array<IRisk>;
        Stakeholders: Array<IStakeholder>;

        /** Determines if this project is an IT digitalization strategy */
        IsStrategy: boolean;

        JointMunicipalProjectId: number;
        JointMunicipalProject: IItProject;
        JointMunicipalProjects: Array<IItProject>;

        CommonPublicProjectId: number;
        CommonPublicProject: IItProject;
        CommonPublicProjects: Array<IItProject>;

        /** Organization Unit responsible for this project. */
        ResponsibleUsage: IItProjectOrgUnitUsage;
        /** Organization units that are using this project. */
        UsedByOrgUnits: Array<IItProjectOrgUnitUsage>;

        /** Gets or sets the associated it system usages. */
        ItSystemUsages: Array<ItSystemUsage.IItSystemUsage>;
        EconomyYears: Array<IEconomyYear>;


        /** Traffic-light dropdown for overall statusr */
        StatusProject: TrafficLight;
        /** Date-for-status-update field */
        StatusDate: Date;
        /** Notes on collected status on project */
        StatusNote: string;

        Phase1: IItProjectPhase;
        Phase2: IItProjectPhase;
        Phase3: IItProjectPhase;
        Phase4: IItProjectPhase;
        Phase5: IItProjectPhase;
        /** The id of current selected phase */
        CurrentPhase: number;

        /** The "milestones and tasks" table. */
        ItProjectStatuses: Array<IItProjectStatus>;


        /** Gets or sets the original it project identifier this it project was "cloned" from. */
        OriginalId: number;
        /** Gets or sets the original it project this it project was "cloned" from. */
        Original: IItProject;
        /** Gets or sets the it projects that were cloned from this it project. */
        Clones: Array<IItProject>;
        GoalStatus: IGoalStatus;
    }
}
