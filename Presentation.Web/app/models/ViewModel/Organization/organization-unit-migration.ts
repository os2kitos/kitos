module Kitos.Models.ViewModel.Organization {
    
    export interface IOrganizationUnitRegistration extends Organization.IHasSelection {
        id: number;
        text: string;
        index?: number;
        targetPageObjectId?: number;
        objectText?: string;

        optionalObjectContext?: any;
    }

    export interface IRegistrationMigrationStateParameters {
        checkIsRootBusy: () => boolean;
        setRootIsBusy: (value: boolean) => void;
        registrationsChanged: () => void;
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