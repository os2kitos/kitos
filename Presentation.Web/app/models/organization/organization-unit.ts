module Kitos.Models {
    import IItSystemUsage = Models.ItSystemUsage.IItSystemUsage;
    import IItSystemUsageOrgUnitUsage = Models.ItSystemUsage.IItSystemUsageOrgUnitUsage;

    export interface IOrganizationUnit extends IEntity {
        Name: string;
        /** EAN number of the department. */
        Ean: number;
        ParentId: number;
        /** Parent department. */
        Parent: IOrganizationUnit;
        Children: Array<IOrganizationUnit>;
        OrganizationId: number;
        /** The organization which the unit belongs to. */
        Organization: IOrganization;
        /** The usage of task on this Organization Unit.Should be a subset of the TaskUsages of the parent department. */
        TaskUsages: Array<ITaskUsage>;
        /** Local tasks that was created in this unit */
        TaskRefs: Array<ITaskRef>;
        OwnedTasks: Array<ITaskRef>;
        /** Gets or sets the delegated system usages. */
        DelegatedSystemUsages: Array<IItSystemUsage>;
        /** Gets or sets it system usages. */
        ItSystemUsages: Array<IItSystemUsage>;
        /** Users which have set this as their default OrganizationUnit. */
        DefaultUsers: Array<IOrganizationRight>;
        /** This Organization Unit is using these IT Systems (Via ItSystemUsage) */
        Using: Array<IItSystemUsageOrgUnitUsage>;
        /** This Organization Unit is using these IT projects */
        ItProjects: Array<ItProject.IItProjectOrgUnitUsage>;
        /** This Organization Unit is responsible for these IT ItContracts */
        ItContracts: Array<ItContract.IItContract>;
        /** The Organization Unit is listed in these economy streams */
        EconomyStreams: Array<ItContract.IEconomyStream>;
    }
}
