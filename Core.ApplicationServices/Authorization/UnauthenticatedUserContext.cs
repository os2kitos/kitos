using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Authorization
{
    public class UnauthenticatedUserContext : IOrganizationalUserContext
    {
        private const int INVALID_ID = -1;

        public int UserId { get; } = INVALID_ID;

        public IEnumerable<int> OrganizationIds => new int[0];

        public bool HasRoleInOrganizationOfType(OrganizationCategory category)
        {
            return false;
        }

        public bool IsGlobalAdmin()
        {
            return false;
        }

        public bool HasRole(int organizationId, OrganizationRole role)
        {
            return false;
        }

        public bool HasRoleIn(int organizationId)
        {
            return false;
        }

        public bool HasRoleInSameOrganizationAs(IEntity entity)
        {
            return false;
        }

        public bool HasAssignedWriteAccess(IEntity entity)
        {
            return false;
        }
    }
}
