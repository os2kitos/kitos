using Presentation.Web.Models.API.V2.Internal.Response.Organizations.Conflicts;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations
{
    public class OrganizationRemovalConflictsResponseDTO
    {
        public IEnumerable<SystemWithUsageOutsideOrganizationConflictResponseDTO> SystemsWithUsagesOutsideTheOrganization { get; set; }
        public IEnumerable<InterfacesExposedOutsideTheOrganizationResponseDTO> InterfacesExposedOnSystemsOutsideTheOrganization { get; set; }
        public IEnumerable<MultipleConflictsResponseDTO> SystemsExposingInterfacesDefinedInOtherOrganizations { get; set; }
        public IEnumerable<MultipleConflictsResponseDTO> SystemsSetAsParentSystemToSystemsInOtherOrganizations { get; set; }
        public IEnumerable<SimpleConflictResponseDTO> DprInOtherOrganizationsWhereOrgIsDataProcessor { get; set; }
        public IEnumerable<SimpleConflictResponseDTO> DprInOtherOrganizationsWhereOrgIsSubDataProcessor { get; set; }
        public IEnumerable<SimpleConflictResponseDTO> ContractsInOtherOrganizationsWhereOrgIsSupplier { get; set; }
        public IEnumerable<SimpleConflictResponseDTO> SystemsInOtherOrganizationsWhereOrgIsRightsHolder { get; set; }
        public IEnumerable<SimpleConflictResponseDTO> SystemsWhereOrgIsArchiveSupplier { get; set; }
    }
}