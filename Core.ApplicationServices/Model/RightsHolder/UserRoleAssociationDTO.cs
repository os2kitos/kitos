using System;
using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Model.RightsHolder
{
    public class UserRoleAssociationDTO
    {
        public OrganizationRole Role { get; }
        public User User { get; }
        public Organization Organization { get; }

        public UserRoleAssociationDTO(OrganizationRole role, User user, Organization organization)
        {
            Role = role;
            User = user ?? throw new ArgumentNullException(nameof(user));
            Organization = organization ?? throw new ArgumentNullException(nameof(organization));
        }
    }
}
