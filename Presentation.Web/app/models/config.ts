module Kitos.Models {
    /** Configuration of KITOS for an organization */
    export interface IConfig extends IEntity {
        ShowItProjectModule: boolean;
        ShowItSystemModule: boolean;
        ShowItContractModule: boolean;
        ShowDataProcessing: boolean;
        ItSupportModuleNameId: number;
        ItSupportGuide: string;
        ShowTabOverview: boolean;
        ShowColumnTechnology: boolean;
        ShowColumnUsage: boolean;
        Organization: IOrganization;
    }
}
