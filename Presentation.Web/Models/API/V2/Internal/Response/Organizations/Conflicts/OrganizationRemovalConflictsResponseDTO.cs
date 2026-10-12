using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations.Conflicts
{
    public class OrganizationRemovalConflictsResponseDTO
    {
        public required IEnumerable<SystemWithUsageOutsideOrganizationConflictResponseDTO> SystemsWithUsagesOutsideTheOrganization { get; set; }
        public required IEnumerable<InterfacesExposedOutsideTheOrganizationResponseDTO> InterfacesExposedOnSystemsOutsideTheOrganization { get; set; }
        public required IEnumerable<MultipleConflictsResponseDTO> SystemsExposingInterfacesDefinedInOtherOrganizations { get; set; }
        public required IEnumerable<MultipleConflictsResponseDTO> SystemsSetAsParentSystemToSystemsInOtherOrganizations { get; set; }
        public required IEnumerable<SimpleConflictResponseDTO> DprInOtherOrganizationsWhereOrgIsDataProcessor { get; set; }
        public required IEnumerable<SimpleConflictResponseDTO> DprInOtherOrganizationsWhereOrgIsSubDataProcessor { get; set; }
        public required IEnumerable<SimpleConflictResponseDTO> ContractsInOtherOrganizationsWhereOrgIsSupplier { get; set; }
        public required IEnumerable<SimpleConflictResponseDTO> SystemsInOtherOrganizationsWhereOrgIsRightsHolder { get; set; }
        public required IEnumerable<SimpleConflictResponseDTO> SystemsWhereOrgIsArchiveSupplier { get; set; }
    }
}