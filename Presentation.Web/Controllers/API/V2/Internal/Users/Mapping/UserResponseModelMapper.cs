using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Organizations;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Internal.Response.User;

namespace Presentation.Web.Controllers.API.V2.Internal.Users.Mapping
{
    public class UserResponseModelMapper : IUserResponseModelMapper
    {
        private readonly IOrganizationService _organizationService;
        public UserResponseModelMapper(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        public Result<UserResponseDTO, OperationError> ToUserResponseDTO(Guid organizationUuid, User user)
        {
            return _organizationService.GetOrganization(organizationUuid)
                .Bind(organization =>
                    Result<OrganizationUnit, OperationError>.Success(
                        _organizationService.GetDefaultUnit(organization, user)))
                .Select(defaultUnit =>
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
                        Roles = user.GetRolesInOrganization(organizationUuid).Select(x => x.ToOrganizationRoleChoice())
                            .ToList(),
                        DefaultOrganizationUnit = defaultUnit?.MapIdentityNamePairDTO(),
                        LastSentAdvis = user.LastAdvisDate
                    };
                });
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