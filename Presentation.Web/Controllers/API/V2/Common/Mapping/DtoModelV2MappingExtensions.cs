using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.Shared.Write;
using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Organization;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public static class DtoModelV2MappingExtensions
    {
        public static IdentityNamePairResponseDTO MapIdentityNamePairDTO<T>(this T source) where T : IHasUuid, IHasName
        {
            return new(source.Uuid, source.Name);
        }

        public static IdentityNamePairWithDeactivatedStatusDTO MapIdentityNamePairWithDeactivatedStatusDTO<T>(this T source) where T : IHasUuid, IHasName, IEntityWithEnabledStatus
        {
            return new(source.Uuid, source.Name, source.Disabled);
        }

        public static IdentityNamePairWithDeactivatedStatusDTO MapIdentityNamePairWithDeactivatedStatusDTO(this ItSystemUsage source)
        {
            return new(source.Uuid, source.ItSystem.Name, source.ItSystem.Disabled);
        }

        public static IdentityNamePairResponseDTO MapIdentityNamePairDTO(this TaskRef source)
        {
            return new(source.Uuid, source.TaskKey);
        }

        public static IdentityNamePairResponseDTO MapIdentityNamePairDTO(this ItSystemUsage source)
        {
            return new(source.Uuid, source.ItSystem.Name);
        }

        public static IdentityNamePairResponseDTO MapIdentityNamePairDTO(this User source)
        {
            return new(source.Uuid, source.GetFullName());
        }

        public static ShallowOrganizationResponseDTO MapShallowOrganizationResponseDTO(this Organization organization)
        {
            return new(organization.Uuid, organization.Name, organization.GetActiveCvr());
        }

        public static UserRolePair ToUserRolePair(this RoleAssignmentRequestDTO src)
        {
            return new UserRolePair(src.UserUuid, src.RoleUuid);
        }

        public static IEnumerable<UserRolePair> ToUserRolePairs(this BulkRoleAssignmentRequestDTO src)
        {
            return src.UserUuids.Select(uuid => new UserRolePair(uuid, src.RoleUuid)).ToList();
        }
    }
}