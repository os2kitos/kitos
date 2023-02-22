using Core.Abstractions.Extensions;
using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Internal.Response.Roles;
using Presentation.Web.Models.API.V2.Internal.Response.User;

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
    }
}