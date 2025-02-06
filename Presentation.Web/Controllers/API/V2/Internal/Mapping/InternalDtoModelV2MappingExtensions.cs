using Core.Abstractions.Extensions;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Internal.Response.Roles;
using Presentation.Web.Models.API.V2.Internal.Response.User;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Presentation.Web.Controllers.API.V2.Internal.Mapping
{
    public static class InternalDtoModelV2MappingExtensions
    {
        public static ExtendedRoleAssignmentResponseDTO MapExtendedRoleAssignmentResponse<TObject, TRight, TRole>(this IRight<TObject, TRight, TRole> right)
            where TObject : HasRightsEntity<TObject, TRight, TRole>
            where TRight : IRight<TObject, TRight, TRole>
            where TRole : IRoleEntity, IHasId, IHasName, IHasUuid
        {
            return new ExtendedRoleAssignmentResponseDTO
            {
                Role = right.Role.MapIdentityNamePairDTO(),
                User = right.User.MapUserReferenceResponseDTO()
            };
        }

        public static UserReferenceResponseDTO MapUserReferenceResponseDTO(this User user)
        {
            return user
                .MapIdentityNamePairDTO()
                .Transform(dto => new UserReferenceResponseDTO(dto.Uuid, dto.Name, user.Email));
        }

        public static IEnumerable<UserReferenceWithOrganizationResponseDTO> MapUserToMultipleLocalAdminResponse(this User user)
        {
            return user.GetOrganizationsWhereRoleIsAssigned(OrganizationRole.LocalAdmin)
                .Select(org => org.MapIdentityNamePairDTO())
                .Select(orgDto => new UserReferenceWithOrganizationResponseDTO(orgDto, user.Uuid, user.GetFullName(), user.Email));
        }

        public static UserReferenceWithOrganizationResponseDTO MapUserToSingleLocalAdminResponse(this User user, Guid organizationUuid)
        {
            var orgDto = user.GetOrganizations().First(x => x.Uuid == organizationUuid).MapIdentityNamePairDTO();
            return new UserReferenceWithOrganizationResponseDTO(orgDto, user.Uuid, user.GetFullName(), user.Email);

        }
    }
}