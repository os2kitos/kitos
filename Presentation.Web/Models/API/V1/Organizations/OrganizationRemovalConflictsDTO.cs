using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class OrganizationRemovalConflictsDTO
    {
        public IEnumerable<SystemWithUsageOutsideOrganizationConflictDTO> SystemsWithUsagesOutsideTheOrganization { get; set; }
        public IEnumerable<InterfaceExposedOnSystemsOutsideTheOrganizationConflictDTO> InterfacesExposedOnSystemsOutsideTheOrganization { get; set; }
        public IEnumerable<SystemExposingInterfacesDefinedInOtherOrganizationsConflictDTO> SystemsExposingInterfacesDefinedInOtherOrganizations { get; set; }
        public IEnumerable<SystemSetAsParentSystemToSystemsInOtherOrganizationsConflictDTO> SystemsSetAsParentSystemToSystemsInOtherOrganizations { get; set; }
        public IEnumerable<EntityWithOrganizationRelationshipDTO> DprInOtherOrganizationsWhereOrgIsDataProcessor { get; set; }
        public IEnumerable<EntityWithOrganizationRelationshipDTO> DprInOtherOrganizationsWhereOrgIsSubDataProcessor { get; set; }
        public IEnumerable<EntityWithOrganizationRelationshipDTO> ContractsInOtherOrganizationsWhereOrgIsSupplier { get; set; }
        public IEnumerable<EntityWithOrganizationRelationshipDTO> SystemsInOtherOrganizationsWhereOrgIsRightsHolder { get; set; }

        public OrganizationRemovalConflictsDTO()
        {
        }

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
    }
}