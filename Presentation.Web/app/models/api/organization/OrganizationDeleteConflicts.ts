module Kitos.Models.Api.Organization {


    export interface InterfaceExposedOnSystemsOutsideTheOrganizationConflictDTO {
        exposedInterface: Models.Generic.NamedEntity.NamedEntityDTO;
        exposedBy: Models.Generic.NamedEntity.EntityWithOrganizationRelationshipDTO;
    }

    export interface SystemWithUsageOutsideOrganizationConflictDTO {
        system: Models.Generic.NamedEntity.NamedEntityDTO;
        otherOrganizationsWhichUseTheSystem: Models.Generic.Organization.ShallowOrganizationDTO[];
    }

    export interface SystemExposingInterfacesDefinedInOtherOrganizationsConflictDTO {
        system: Models.Generic.NamedEntity.NamedEntityDTO;
        exposedInterfaces: Models.Generic.NamedEntity.EntityWithOrganizationRelationshipDTO[];
    }

    export interface SystemSetAsParentSystemToSystemsInOtherOrganizationsConflictDTO {
        system: Models.Generic.NamedEntity.NamedEntityDTO;
        children: Models.Generic.NamedEntity.EntityWithOrganizationRelationshipDTO[];
    }

    export interface OrganizationDeleteConflicts {
        systemsWithUsagesOutsideTheOrganization: SystemWithUsageOutsideOrganizationConflictDTO[];
        interfacesExposedOnSystemsOutsideTheOrganization: InterfaceExposedOnSystemsOutsideTheOrganizationConflictDTO[];
        systemsExposingInterfacesDefinedInOtherOrganizations: SystemExposingInterfacesDefinedInOtherOrganizationsConflictDTO[];
        systemsSetAsParentSystemToSystemsInOtherOrganizations: SystemSetAsParentSystemToSystemsInOtherOrganizationsConflictDTO[];
        dprInOtherOrganizationsWhereOrgIsDataProcessor: Models.Generic.NamedEntity.EntityWithOrganizationRelationshipDTO[];
        dprInOtherOrganizationsWhereOrgIsSubDataProcessor: Models.Generic.NamedEntity.EntityWithOrganizationRelationshipDTO[];
        contractsInOtherOrganizationsWhereOrgIsSupplier: Models.Generic.NamedEntity.EntityWithOrganizationRelationshipDTO[];
        systemsInOtherOrganizationsWhereOrgIsRightsHolder: Models.Generic.NamedEntity.EntityWithOrganizationRelationshipDTO[];
    }

    export function detectConflicts(conflicts: OrganizationDeleteConflicts): boolean {
        return conflicts.systemsWithUsagesOutsideTheOrganization.length > 0 ||
            conflicts.interfacesExposedOnSystemsOutsideTheOrganization.length > 0 ||
            conflicts.systemsExposingInterfacesDefinedInOtherOrganizations.length > 0 ||
            conflicts.systemsSetAsParentSystemToSystemsInOtherOrganizations.length > 0 ||
            conflicts.dprInOtherOrganizationsWhereOrgIsDataProcessor.length > 0 ||
            conflicts.dprInOtherOrganizationsWhereOrgIsSubDataProcessor.length > 0 ||
            conflicts.contractsInOtherOrganizationsWhereOrgIsSupplier.length > 0 ||
            conflicts.systemsInOtherOrganizationsWhereOrgIsRightsHolder.length > 0;
    }
}