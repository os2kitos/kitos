using System;
using System.Net;
using Core.ApplicationServices.Organizations;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;

namespace Presentation.Web.Controllers.API.V2.Internal.Organizations
{
    /// <summary>
    /// Internal API for the organizations in KITOS
    /// </summary>
    [RoutePrefix("api/v2/internal/organizations")]
    public class OrganizationsInternalV2Controller : InternalApiV2Controller
    {
        private readonly IOrganizationService _organizationService;
        private readonly IResourcePermissionsResponseMapper _permissionsResponseMapper;
        private readonly IOrganizationMapper _organizationMapper;

        public OrganizationsInternalV2Controller(IOrganizationService organizationService, IResourcePermissionsResponseMapper permissionsResponseMapper, IOrganizationMapper organizationMapper)
        {
            _organizationService = organizationService;
            _permissionsResponseMapper = permissionsResponseMapper;
            _organizationMapper = organizationMapper;
        }

        [Route("{organizationUuid}/permissions")]
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

        [HttpPatch]
        [Route("{organizationUuid}/masterData")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationMasterDataResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult UpdateOrganizationMasterData([FromUri] [NonEmptyGuid] Guid organizationUuid, OrganizationMasterDataRequestDTO requestDto)
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var updateParameters = _organizationMapper.ToMasterDataUpdateParameters(requestDto);
            return _organizationService.UpdateOrganizationMasterData(organizationUuid, updateParameters)
                .Select(_organizationMapper.ToOrganizationDTO)
                .Match(Ok, FromOperationError);
        }

        [Route("{organizationUuid}/masterData")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationMasterDataResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetOrganizationMasterData([FromUri] [NonEmptyGuid] Guid organizationUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _organizationService.GetOrganizationMasterData(organizationUuid)
                .Select(_organizationMapper.ToMasterDataDTO)
                .Match(Ok, FromOperationError);
        }

        [Route("{organizationUuid}/masterData/roles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationMasterDataRolesResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetOrganizationMasterDataRoles([FromUri][NonEmptyGuid] Guid organizationUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _organizationService.GetOrganizationMasterDataRoles(organizationUuid)
                .Select(_organizationMapper.ToRolesDTO)
                .Match(Ok, FromOperationError);
        }
        
        [HttpPatch]
        [Route("{organizationUuid}/masterData/roles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationMasterDataRolesResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult UpsertOrganizationMasterDataRoles([FromUri][NonEmptyGuid] Guid organizationUuid, OrganizationMasterDataRolesRequestDTO requestDto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParameters = _organizationMapper.ToMasterDataRolesUpdateParameters(organizationUuid, requestDto);
            return _organizationService.UpsertOrganizationMasterDataRoles(organizationUuid, updateParameters)
                .Select(_organizationMapper.ToRolesDTO)
                .Match(Ok, FromOperationError);
        }
    }
}
