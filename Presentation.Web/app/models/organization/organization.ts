module Kitos.Models {
    /** Represents an Organization (such as a municipality, or a company).Holds local configuration and admin roles, as well as collections ofItSystems, ItProjects, etc that was created in this organization. */
    export interface IOrganization extends IEntity {
        Name: string;
        TypeId: number;
        /** Cvr number */
        Cvr: string;
        AccessModifier: AccessModifier;
        Uuid: any;
        OrgUnits: Array<IOrganizationUnit>;
        /** ItProjects created inside this organization */
        ItProjects: Array<ItProject.IItProject>;
        BelongingSystems: Array<ItSystem.IItSystem>;
        ItSystems: Array<ItSystem.IItSystem>;
        ItInterfaces: Array<ItSystem.IItInterface>;
        /** Organization is marked as supplier in these contracts */
        Supplier: Array<ItContract.IItContract>;
        /** ItContracts created inside the organization */
        ItContracts: Array<ItContract.IItContract>;
        /** Local usages of IT systems within this organization */
        ItSystemUsages: Array<ItSystemUsage.IItSystemUsage>;
        /** Local configuration of KITOS */
        Config: IConfig;
        Type: OrganizationType;

        ContactPersonId: number;
        ContactPerson: IUser;
    }
}
