module Kitos.Models.ViewModel.Organization {
    
    export interface IOrganizationUnitRegistration extends Organization.IHasSelection {
        id: number;
        text: string;
        index?: number;
        objectText?: string;

        targetUnitId?: number;
        optionalObjectContext?: any;
    }

    export enum OrganizationRegistrationOption {
        Roles = 0,
        InternalPayments = 1,
        ExternalPayments = 2,
        ContractRegistrations = 3,
        ResponsibleSystems = 4,
        RelevantSystems = 5
    }
}