module Kitos.Models {
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
        OwnedTasks: Array<ITaskRef>;
        /** Users which have set this as their default OrganizationUnit. */
        DefaultUsers: Array<IOrganizationRight>;
        /** This Organization Unit is using these IT Systems (Via ItSystemUsage) */
        Using: Array<IItSystemUsageOrgUnitUsage>;
        /** This Organization Unit is responsible for these IT ItContracts */
        ItContracts: Array<ItContract.IItContract>;
        /** The Organization Unit is listed in these economy streams */
        EconomyStreams: Array<ItContract.IEconomyStream>;
    }
}
