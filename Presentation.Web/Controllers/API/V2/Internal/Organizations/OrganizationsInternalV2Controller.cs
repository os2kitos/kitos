using System;
using System.Net;
using Core.ApplicationServices.Organizations;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.Internal.Response.Roles;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.ApplicationServices;
using Core.ApplicationServices.Model.Users;
using Core.ApplicationServices.Rights;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;

namespace Presentation.Web.Controllers.API.V2.Internal.Organizations
{
    /// <summary>
    /// Internal API for the organizations in KITOS
    /// </summary>
    [RoutePrefix("api/v2/internal/organizations/{organizationUuid}")]
    public class OrganizationsInternalV2Controller : InternalApiV2Controller
    {
        private readonly IOrganizationService _organizationService;
        private readonly IUserRightsService _userRightsService;
        private readonly IEntityIdentityResolver _entityIdentityResolver;
        private readonly IResourcePermissionsResponseMapper _permissionsResponseMapper;

        public OrganizationsInternalV2Controller(IOrganizationService organizationService, IResourcePermissionsResponseMapper permissionsResponseMapper, IUserRightsService userRightsService, IEntityIdentityResolver identity)
        {
            _organizationService = organizationService;
            _permissionsResponseMapper = permissionsResponseMapper;
            _userRightsService = userRightsService;
            _entityIdentityResolver = identity;
        }

        [Route("permissions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ResourcePermissionsResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetPermissions([NonEmptyGuid] Guid organizationUuid)
        {
            return _organizationService.GetPermissions(organizationUuid)
                .Select(_permissionsResponseMapper.Map)
                .Match(Ok, FromOperationError);
        }

        [Route("user/{userUuid}/rights")]
        public IHttpActionResult GetUserRights([NonEmptyGuid] Guid organizationUuid, [NonEmptyGuid] Guid userUuid)
        {
            var orgIdRes = _entityIdentityResolver.ResolveDbId<Organization>(organizationUuid);
            var userIdRes = _entityIdentityResolver.ResolveDbId<User>(userUuid);
            if (orgIdRes.IsNone || userIdRes.IsNone)
            {
                return BadRequest("The provided UUID's could not be resolved");
            }

            var orgId = orgIdRes.Value;
            var userId = userIdRes.Value;
            return _userRightsService.GetUserRights(userId, orgId)
                .Select(mapUserRightAssignmentsToDTO)
                .Match(Ok, FromOperationError);
        }

        private object mapUserRightAssignmentsToDTO(UserRightsAssignments userRights)
        {
            return userRights;
        }
    }
}