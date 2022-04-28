using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class SystemWithUsageOutsideOrganizationConflictDTO
    {
        public NamedEntityDTO System { get; }
        public IEnumerable<ShallowOrganizationDTO> OtherOrganizationsWhichUseTheSystem { get; }

        public SystemWithUsageOutsideOrganizationConflictDTO(NamedEntityDTO system, IEnumerable<ShallowOrganizationDTO> otherOrganizationsWhichUseTheSystem)
        {
            System = system;
            OtherOrganizationsWhichUseTheSystem = otherOrganizationsWhichUseTheSystem;
        }
    }
}