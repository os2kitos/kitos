module Kitos.Models.Api.Organization {
    export class OrganizationRegistrationDetailsDto {
        id: number;
        text: string;
        type: OrganizationRegistrationOption;
        objectId?: number;
        objectName: string;
        paymentIndex?: number;
    }

    export class OrganizationRegistrationChangeRequest {
        id: number;
        type: OrganizationRegistrationOption;
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