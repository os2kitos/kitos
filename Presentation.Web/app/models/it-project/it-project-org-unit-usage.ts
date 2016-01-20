module Kitos.Models.ItProject {
    export interface IItProjectOrgUnitUsage {
        ItProjectId: number;
        ItProject: IItProject;
        OrganizationUnitId: number;
        OrganizationUnit: IOrganizationUnit;
    }
}
