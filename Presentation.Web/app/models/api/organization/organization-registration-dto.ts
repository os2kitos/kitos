module Kitos.Models.Api.Organization {
    export class OrganizationUnitRegistrationDetailsDto {
        itContractRegistrations: Generic.NamedEntity.NamedEntityDTO[];
        organizationUnitRights: Generic.NamedEntity.NamedEntityWithUserFullNameDTO[];
        payments: PaymentRegistrationDetailsDto[];
        relevantSystems: Generic.NamedEntity.NamedEntityWithEnabledStatusDTO[];
        responsibleSystems: Generic.NamedEntity.NamedEntityWithEnabledStatusDTO[];
    }

    export class PaymentRegistrationDetailsDto {
        itContract: Generic.NamedEntity.NamedEntityDTO;
        externalPayments: Generic.NamedEntity.NamedEntityDTO[];
        internalPayments: Generic.NamedEntity.NamedEntityDTO[];
    }

    export class OrganizationUnitRegistrationChangeRequestDto {
        itContractRegistrations: number[];
        organizationUnitRights: number[];
        paymentRegistrationDetails: PaymentRegistrationChangeRequestDto[];
        relevantSystems: number[];
        responsibleSystems: number[];
    }

    export class TransferOrganizationUnitRegistrationRequestDto extends OrganizationUnitRegistrationChangeRequestDto {
        targetUnitUuid: string;
    }

    export interface PaymentRegistrationChangeRequestDto {
        itContractId: number
        externalPayments: number[]
        internalPayments: number[]
    }

    export interface UnitAccessRightsDto {
        canBeRead: boolean
        canBeModified: boolean
        canBeDeleted: boolean
        canNameBeModified: boolean
        canEanBeModified: boolean
        canDeviceIdBeModified: boolean
        canBeRearranged: boolean
        canEditRegistrations: boolean
}

    export interface UnitAccessRightsWithUnitIdDto extends UnitAccessRightsDto {
        unitId: number;
    }
}