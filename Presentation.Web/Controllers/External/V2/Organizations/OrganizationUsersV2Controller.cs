using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Infrastructure.Services.Types;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response.Organization;
using Presentation.Web.Models.External.V2.Types;
using Serilog;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2.Organizations
{
    [RoutePrefix("api/v2")]
    [DenyRightsHoldersAccess]
    public class OrganizationUsersV2Controller : ExternalBaseController
    {
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        public OrganizationUsersV2Controller(IUserService userService, ILogger logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Returns the users of an organization if the authenticated is a member of the organization.
        /// </summary>
        /// <param name="organizationUuid">UUID of the organization</param>
        /// <param name="nameOrEmailQuery">Query by text in name or email</param>
        /// <param name="roleQuery">Query by role assignment</param>
        /// <returns>A list og users in a specific organizational context</returns>
        [HttpGet]
        [Route("organization-users")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<OrganizationUserResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [DenyRightsHoldersAccess]
        public IHttpActionResult GetOrganizationUsers(
            [NonEmptyGuid] Guid organizationUuid,
            string nameOrEmailQuery = null,
            OrganizationUserRole? roleQuery = null,
            [FromUri] StandardPaginationQuery paginationQuery = null)
        {
            //TODO: Apply queries
            //TODO: Role search



            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the a specific user within an organization
        /// </summary>
        /// <param name="userUuid">UUID of the user entity in KITOS</param>
        /// <param name="organizationUuid">UUID of the organization</param>
        /// <returns>A user in the context of a specific organization</returns>
        [HttpGet]
        [Route("organization-users/{userUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationUserResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [DenyRightsHoldersAccess]
        public IHttpActionResult GetOrganizationUser([NonEmptyGuid] Guid userUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return _userService
                .GetUserInOrganization(organizationUuid, userUuid)
                .Select(user => (organizationUuid, user))
                .Select(ToDTO)
                .Match(Ok, FromOperationError);
        }

        private OrganizationUserResponseDTO ToDTO((Guid organizationUuid, User user) context)
        {
            return new(
                context.user.Uuid,
                context.user.GetFullName(),
                context.user.Name,
                context.user.LastName,
                context.user.Email,
                context.user.HasApiAccess == true,
                context.user.PhoneNumber, MapRoles(context));
        }

        /// <summary>
        /// NOTE: Global admin is intentionally left out. It's a KITOS-only term and should not be exported.
        /// </summary>
        private static readonly IReadOnlyDictionary<OrganizationRole, OrganizationUserRole> DomainRoleToApiRoleMap = new ReadOnlyDictionary<OrganizationRole, OrganizationUserRole>(new Dictionary<OrganizationRole, OrganizationUserRole>
                {
                    {OrganizationRole.User, OrganizationUserRole.User},
                    {OrganizationRole.LocalAdmin, OrganizationUserRole.LocalAdmin},
                    {OrganizationRole.OrganizationModuleAdmin, OrganizationUserRole.OrganizationModuleAdmin},
                    {OrganizationRole.ProjectModuleAdmin, OrganizationUserRole.ProjectModuleAdmin},
                    {OrganizationRole.SystemModuleAdmin, OrganizationUserRole.SystemModuleAdmin},
                    {OrganizationRole.ContractModuleAdmin, OrganizationUserRole.ContractModuleAdmin},
                    {OrganizationRole.ReportModuleAdmin, OrganizationUserRole.ReportModuleAdmin},
                    {OrganizationRole.RightsHolderAccess, OrganizationUserRole.RightsHolderAccess}
                });
        private IEnumerable<OrganizationUserRole> MapRoles((Guid organizationUuid, User user) context)
        {
            return from organizationRole in context.user.GetRolesInOrganization(context.organizationUuid)
                   select MapRole(organizationRole)
                into role
                   where role.HasValue
                   select role.Value;
        }

        private Maybe<OrganizationUserRole> MapRole(OrganizationRole role)
        {
            if (!Ignore(role))
            {
                if (DomainRoleToApiRoleMap.TryGetValue(role, out var exportedRole))
                {
                    return exportedRole;
                }

                _logger.Error("ERROR: Failed mapping org role:{role} to an exported role", role.ToString("G"));
            }
            return Maybe<OrganizationUserRole>.None;
        }

        private static bool Ignore(OrganizationRole organizationRole)
        {
            return organizationRole == OrganizationRole.GlobalAdmin;
        }
    }
}