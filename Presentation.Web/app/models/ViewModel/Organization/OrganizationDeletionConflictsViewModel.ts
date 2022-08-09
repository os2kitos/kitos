module Kitos.Models.ViewModel.Organization {

    export interface WithName {
        name: string;
    }

    export interface EntityFromOrganization extends WithName {
        organizationName: string;
    }

    export interface ItInterfaceExposureConflict extends WithName {
        exposedOnSystem: EntityFromOrganization;
    }

    export interface ItSystemUsageConflicts extends WithName {
        organizations: string[];
    }

    export interface ItSystemInterfaceExposureConflicts extends WithName {
        exposedInterfaces: Array<EntityFromOrganization>;
    }

    export interface ItSystemChildrenInOtherOrgConflicts extends WithName {
        children: Array<EntityFromOrganization>;
    }

    export interface OrganizationDeletionConflictsViewModel {
        interfacesBeingMovedToDefaultOrg: {
            exposedOnSystemsInOtherOrganizations: Array<ItInterfaceExposureConflict>;
        },
        systemsBeingMovedToDefaultOrg: {
            systemsUsedOutsideTheOrganization: Array<ItSystemUsageConflicts>,
            systemsExposingInterfacesOutsideTheOrganization: Array<ItSystemInterfaceExposureConflicts>;
            systemsSetAsParentsToSystemsOutsideTheOrganization: Array<ItSystemChildrenInOtherOrgConflicts>;
            systemsWhereOrgIsArchiveSupplier: Array<EntityFromOrganization>;
        },
        contractsWhereSupplierWillBeRemoved: Array<EntityFromOrganization>;
        dprWhereOrganizationIsRemovedFromListOfDataProcessors: Array<EntityFromOrganization>;
        dprWhereOrganizationIsRemovedFromListOfSubDataProcessors: Array<EntityFromOrganization>;
        systemsInOtherOrganizationsWhereRightsHolderWillBeRemoved: Array<EntityFromOrganization>;
    }
}