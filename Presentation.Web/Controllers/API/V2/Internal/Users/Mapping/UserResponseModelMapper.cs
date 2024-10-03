using System;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Response.User;

namespace Presentation.Web.Controllers.API.V2.Internal.Users.Mapping
{
    public class UserResponseModelMapper : IUserResponseModelMapper
    {
        public UserResponseDTO ToUserResponseDTO(Guid organizationUuid, User user)
        {
            return new UserResponseDTO
            {
                Uuid = user.Uuid,
                Email = user.Email,
                FirstName = user.Name,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                DefaultUserStartPreference =
                    DefaultUserStartPreferenceChoiceMapper.GetDefaultUserStartPreferenceChoice(
                        user.DefaultUserStartPreference),
                HasApiAccess = user.HasApiAccess,
                HasStakeHolderAccess = user.HasStakeHolderAccess,
                Roles = user.GetRolesInOrganization(organizationUuid).Select(x => x.ToOrganizationRoleChoice()).ToList(),
                LastSentAdvis = user.LastAdvisDate
            };
        }
        public UserIsPartOfCurrentOrgResponseDTO ToUserWithIsPartOfCurrentOrgResponseDTO(Guid organizationUuid, User user)
        {
            if (user == null)
            {
                return null;
            }

            return new UserIsPartOfCurrentOrgResponseDTO
            {
                Uuid = user.Uuid,
                IsPartOfCurrentOrganization = user.GetOrganizations().Any(x => x.Uuid == organizationUuid)
            };
        }
    }
}