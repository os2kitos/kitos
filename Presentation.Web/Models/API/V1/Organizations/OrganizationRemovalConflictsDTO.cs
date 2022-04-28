using System.Collections.Generic;
using System.Linq;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class OrganizationRemovalConflictsDTO
    {
        public IEnumerable<SystemWithUsageOutsideOrganizationConflictDTO> SystemsWithUsagesOutsideTheOrganization { get; }
        public IEnumerable<InterfaceExposedOnSystemsOutsideTheOrganizationConflictDTO> InterfacesExposedOnSystemsOutsideTheOrganization { get; }
        public IEnumerable<SystemExposingInterfacesDefinedInOtherOrganizationsConflictDTO> SystemsExposingInterfacesDefinedInOtherOrganizations { get; }
        public IEnumerable<SystemSetAsParentSystemToSystemsInOtherOrganizationsConflictDTO> SystemsSetAsParentSystemToSystemsInOtherOrganizations { get; }
        public IEnumerable<EntityWithOrganizationRelationshipDTO> DprInOtherOrganizationsWhereOrgIsDataProcessor { get; }
        public IEnumerable<EntityWithOrganizationRelationshipDTO> DprInOtherOrganizationsWhereOrgIsSubDataProcessor { get; }
        public IEnumerable<EntityWithOrganizationRelationshipDTO> ContractsInOtherOrganizationsWhereOrgIsSupplier { get; }
        public IEnumerable<EntityWithOrganizationRelationshipDTO> SystemsInOtherOrganizationsWhereOrgIsRightsHolder { get; }

        public OrganizationRemovalConflictsDTO(
            IEnumerable<SystemWithUsageOutsideOrganizationConflictDTO> systemsWithUsagesOutsideTheOrganization,
            IEnumerable<InterfaceExposedOnSystemsOutsideTheOrganizationConflictDTO> interfacesExposedOnSystemsOutsideTheOrganization,
            IEnumerable<SystemExposingInterfacesDefinedInOtherOrganizationsConflictDTO> systemsExposingInterfacesDefinedInOtherOrganizations,
            IEnumerable<SystemSetAsParentSystemToSystemsInOtherOrganizationsConflictDTO> systemsSetAsParentSystemToSystemsInOtherOrganizations,
            IEnumerable<EntityWithOrganizationRelationshipDTO> dprInOtherOrganizationsWhereOrgIsDataProcessor,
            IEnumerable<EntityWithOrganizationRelationshipDTO> dprInOtherOrganizationsWhereOrgIsSubDataProcessor,
            IEnumerable<EntityWithOrganizationRelationshipDTO> contractsInOtherOrganizationsWhereOrgIsSupplier,
            IEnumerable<EntityWithOrganizationRelationshipDTO> systemsInOtherOrganizationsWhereOrgIsRightsHolder)
        {
            SystemsWithUsagesOutsideTheOrganization = systemsWithUsagesOutsideTheOrganization;
            InterfacesExposedOnSystemsOutsideTheOrganization = interfacesExposedOnSystemsOutsideTheOrganization;
            SystemsExposingInterfacesDefinedInOtherOrganizations = systemsExposingInterfacesDefinedInOtherOrganizations;
            SystemsSetAsParentSystemToSystemsInOtherOrganizations = systemsSetAsParentSystemToSystemsInOtherOrganizations;
            DprInOtherOrganizationsWhereOrgIsDataProcessor = dprInOtherOrganizationsWhereOrgIsDataProcessor;
            DprInOtherOrganizationsWhereOrgIsSubDataProcessor = dprInOtherOrganizationsWhereOrgIsSubDataProcessor;
            ContractsInOtherOrganizationsWhereOrgIsSupplier = contractsInOtherOrganizationsWhereOrgIsSupplier;
            SystemsInOtherOrganizationsWhereOrgIsRightsHolder = systemsInOtherOrganizationsWhereOrgIsRightsHolder;
        }

        public bool Any => SystemsWithUsagesOutsideTheOrganization.Any() ||
                           InterfacesExposedOnSystemsOutsideTheOrganization.Any() ||
                           SystemsExposingInterfacesDefinedInOtherOrganizations.Any() ||
                           SystemsSetAsParentSystemToSystemsInOtherOrganizations.Any() ||
                           DprInOtherOrganizationsWhereOrgIsDataProcessor.Any() ||
                           DprInOtherOrganizationsWhereOrgIsSubDataProcessor.Any() ||
                           ContractsInOtherOrganizationsWhereOrgIsSupplier.Any() ||
                           SystemsInOtherOrganizationsWhereOrgIsRightsHolder.Any();

    }
}