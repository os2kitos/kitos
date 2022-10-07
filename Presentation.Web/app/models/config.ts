module Kitos.Models {
    /** Configuration of KITOS for an organization */
    export interface IConfig extends IEntity {
        ShowItSystemModule: boolean;
        ShowItContractModule: boolean;
        ShowDataProcessing: boolean;
        ItSupportModuleNameId: number;
        ItSupportGuide: string;
        Organization: IOrganization;
    }
}
