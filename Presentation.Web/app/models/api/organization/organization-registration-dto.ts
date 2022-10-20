module Kitos.Models.Api.Organization {
    export class OrganizationRegistrationRootDto {
        roles: Array<OrganizationRegistrationDetailsDto>;
        internalPayments: Array<OrganizationRegistrationContractPaymentDto>;
        externalPayments: Array<OrganizationRegistrationContractPaymentDto>;
        contractRegistrations: Array<OrganizationRegistrationDetailsDto>;
        relevantSystemRegistrations: Array<OrganizationRegistrationDetailsDto>;
        responsibleSystemRegistrations: Array<OrganizationRegistrationDetailsDto>;
    }

    export class OrganizationRegistrationDetailsDto {
        id: number;
        text: string;
    }

    export class OrganizationRegistrationContractPaymentDto extends OrganizationRegistrationDetailsDto {
        objectId: number;
        objectName: string;
        paymentIndex: number;
    }

    export class OrganizationRegistrationChangeRequest {
        roles: number[];
        externalPayments: number[];
        internalPayments: number[];
        contractRegistrations: number[];
        responsibleSystems: number[];
        relevantSystems: number[];
    }
}