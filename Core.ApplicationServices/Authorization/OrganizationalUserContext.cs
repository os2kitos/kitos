using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Authorization
{
    /// <summary>
    /// Determines the user in a specific organizational context
    /// </summary>
    public class OrganizationalUserContext : IOrganizationalUserContext
    {
        private readonly ISet<OrganizationRole> _roles;

        public OrganizationalUserContext(
            IEnumerable<OrganizationRole> roles,
            User user,
            int activeOrganizationId)
        {
            UserEntity = user;
            ActiveOrganizationId = activeOrganizationId;
            _roles = new HashSet<OrganizationRole>(roles);
        }

        public int ActiveOrganizationId { get; }

        public int UserId => UserEntity.Id;

        public User UserEntity { get; }

        public bool IsActiveInOrganizationOfType(OrganizationCategory category)
        {
            return UserEntity.DefaultOrganization?.Type?.Category == category;
        }

        public bool HasRole(OrganizationRole role)
        {
            return _roles.Contains(role);
        }

        public bool IsActiveInOrganization(int organizationId)
        {
            return ActiveOrganizationId == organizationId;
        }

        public bool IsActiveInSameOrganizationAs(IEntity entity)
        {
            switch (entity)
            {
                //Prefer match on hasOrganization first since it has static knowledge of organization relationship
                case IOwnedByOrganization hasOrg:
                    return IsActiveInOrganization(hasOrg.OrganizationId);
                case IIsPartOfOrganization organizationRelationship:
                    return organizationRelationship.IsPartOfOrganization(ActiveOrganizationId);
                case IContextAware contextAware:
                    return contextAware.IsInContext(ActiveOrganizationId);
                default:
                    return false;
            }
        }

        public bool HasAssignedWriteAccess(IEntity entity)
        {
            return entity.HasUserWriteAccess(UserEntity);
        }

        public bool HasOwnership(IEntity entity)
        {
            return entity.ObjectOwnerId == UserId;
        }
    }
}