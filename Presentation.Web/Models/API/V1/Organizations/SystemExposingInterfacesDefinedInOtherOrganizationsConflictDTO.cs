using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class SystemExposingInterfacesDefinedInOtherOrganizationsConflictDTO
    {
        public NamedEntityDTO System { get; }
        public IEnumerable<EntityWithOrganizationRelationshipDTO> ExposedInterfaces { get; }

        public SystemExposingInterfacesDefinedInOtherOrganizationsConflictDTO(NamedEntityDTO system, IEnumerable<EntityWithOrganizationRelationshipDTO> exposedInterfaces)
        {
            System = system;
            ExposedInterfaces = exposedInterfaces;
        }
    }
}