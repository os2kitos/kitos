module Kitos.Models.Api.Organization {
    export class OrganizationRegistrationDetailsDto {
        itContractRegistrations: Generic.NamedEntity.NamedEntityDTO[];
        organizationUnitRights: Generic.NamedEntity.NamedEntityDTO[];
        payments: PaymentRegistrationDetailsDto[];
        relevantSystems: Generic.NamedEntity.NamedEntityWithEnabledStatusDTO[];
        responsibleSystems: Generic.NamedEntity.NamedEntityWithEnabledStatusDTO[];
    }

    export class PaymentRegistrationDetailsDto {
        itContract: Generic.NamedEntity.NamedEntityDTO;
        externalPayments: Generic.NamedEntity.NamedEntityDTO[];
        internalPayments: Generic.NamedEntity.NamedEntityDTO[];
    }

    export class OrganizationRegistrationChangeRequest {
        itContractRegistrations: number[];
        organizationUnitRights: number[];
        paymentRegistrationDetails: PaymentRegistrationChangeRequest[];
        relevantSystems: number[];
        responsibleSystems: number[];
    }

    export class PaymentRegistrationChangeRequest {
        itContractId: number;
        externalPayments: number[];
        internalPayments: number[];
    }
}