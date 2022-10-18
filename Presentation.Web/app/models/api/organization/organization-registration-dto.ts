module Kitos.Models.Api.Organization {
    export interface IRelevantSystem {
        systemId: number;
        relevantUnitIds: number[];
    }
    export class OrganizationRegistrationRootDto {
        roles: Array<OrganizationRegistrationDetailsDto>;
        internalPayments: Array<OrganizationRegistrationContractPaymentDto>;
        externalPayments: Array<OrganizationRegistrationContractPaymentDto>;
        contractRegistrations: Array<OrganizationRegistrationDetailsDto>;
        relevantSystemRegistrations: Array<OrganizationRegistrationDetailsWithObjectDataDto>;
        responsibleSystemRegistrations: Array<OrganizationRegistrationDetailsDto>;
    }

    export class OrganizationRegistrationDetailsDto {
        id: number;
        text: string;
    }

    export class OrganizationRegistrationDetailsWithObjectDataDto extends OrganizationRegistrationDetailsDto{
        objectId: number;
        objectName: string;
    }

    export class OrganizationRegistrationContractPaymentDto extends OrganizationRegistrationDetailsWithObjectDataDto{
        paymentIndex: number;
    }

    export class OrganizationRegistrationDeleteRequest {
        roles: number[];
        externalPayments: number[];
        internalPayments: number[];
        contractRegistrations: number[];
        responsibleSystems: number[];
        relevantSystems: IRelevantSystem[];
    }
}