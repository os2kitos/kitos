using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class SystemSetAsParentSystemToSystemsInOtherOrganizationsConflictDTO
    {
        public NamedEntityDTO System { get; }
        public IEnumerable<EntityWithOrganizationRelationshipDTO> Children { get; }

        public SystemSetAsParentSystemToSystemsInOtherOrganizationsConflictDTO(NamedEntityDTO system, IEnumerable<EntityWithOrganizationRelationshipDTO> children)
        {
            System = system;
            Children = children;
        }
    }
}