using Core.ApplicationServices.Organizations;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System;
using System.Linq;
using System.Web.Http;
using Core.ApplicationServices.Organizations.Write;
using Presentation.Web.Models.API.V2.Internal.Response.OrganizationUnit;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Controllers.API.V2.Internal.OrganizationUnits.Mapping;
using Presentation.Web.Models.API.V2.Request.OrganizationUnit;
using System.Web.Http.Results;
using System.Collections.Generic;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;

namespace Presentation.Web.Controllers.API.V2.Internal.OrganizationUnits
{
    /// <summary>
    /// Internal API for the organization units in KITOS
    /// </summary>
    [RoutePrefix("api/v2/internal/organizations/{organizationUuid}/organization-units")]
    public class OrganizationUnitsInternalV2Controller : InternalApiV2Controller
    {
        private readonly IOrganizationUnitWriteService _organizationUnitWriteService;
        private readonly IOrganizationUnitService _organizationUnitService;
        private readonly IOrganizationUnitWriteModelMapper _organizationUnitWriteModelMapper;
        private readonly IOrganizationUnitResponseModelMapper _responseMapper;

        public OrganizationUnitsInternalV2Controller(IOrganizationUnitWriteService organizationUnitWriteService,
            IOrganizationUnitWriteModelMapper organizationUnitWriteModelMapper,
            IOrganizationUnitService organizationUnitService, 
            IOrganizationUnitResponseModelMapper responseMapper)
        {
            _organizationUnitWriteService = organizationUnitWriteService;
            _organizationUnitWriteModelMapper = organizationUnitWriteModelMapper;
            _organizationUnitService = organizationUnitService;
            _responseMapper = responseMapper;
        }

        [Route("{unitUuid}/permissions")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(UnitAccessRightsResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetPermissions([NonEmptyGuid] Guid organizationUuid, [NonEmptyGuid] Guid unitUuid)
        {
            return _organizationUnitService.GetAccessRights(organizationUuid, unitUuid)
                .Select(accessRights => new UnitAccessRightsResponseDTO(accessRights))
                .Match(Ok, FromOperationError);
        }

        [Route("all/collection-permissions")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(UnitAccessRightsWithUnitDataResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetCollectionPermissions([NonEmptyGuid] Guid organizationUuid)
        {
            return _organizationUnitService.GetAccessRightsByOrganization(organizationUuid)
                .Select(accessRightsWithUnits => accessRightsWithUnits.Select(accessRightWithUnit => 
                    new UnitAccessRightsWithUnitDataResponseDTO
                    (
                        accessRightWithUnit.UnitAccessRights,
                        _responseMapper.ToUnitDto(accessRightWithUnit.OrganizationUnit)
                    )
                ))
                .Match(Ok, FromOperationError);
        }

        [Route("create")]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(OrganizationUnitResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult CreateUnit([NonEmptyGuid] Guid organizationUuid, [FromBody] CreateOrganizationUnitRequestDTO parameters)
        {
            return _organizationUnitWriteService.Create(organizationUuid, _organizationUnitWriteModelMapper.FromPOST(parameters))
                .Select(_responseMapper.ToUnitDto)
                .Match(MapUnitCreatedResponse, FromOperationError);
        }

        [Route("{organizationUnitUuid}/patch")]
        [HttpPatch]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(OrganizationUnitResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult PatchUnit([NonEmptyGuid] Guid organizationUuid, [NonEmptyGuid] Guid organizationUnitUuid, [FromBody] UpdateOrganizationUnitRequestDTO parameters)
        {
            return _organizationUnitWriteService.Patch(organizationUuid, organizationUnitUuid,
                    _organizationUnitWriteModelMapper.FromPATCH(parameters))
                .Select(_responseMapper.ToUnitDto)
                .Match(Ok, FromOperationError);
        }

        [Route("{organizationUnitUuid}/delete")]
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult DeleteUnit([NonEmptyGuid] Guid organizationUuid, [NonEmptyGuid] Guid organizationUnitUuid)
        {
            var result = _organizationUnitService.Delete(organizationUuid, organizationUnitUuid);
            return result.HasValue ? FromOperationError(result.Value) : Ok();
        }

        [HttpGet]
        [Route("{organizationUnitUuid}/roles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<OrganizationUnitRolesResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetRoleAssignments([NonEmptyGuid] Guid organizationUuid, [NonEmptyGuid] Guid organizationUnitUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _organizationUnitService.GetRightsOfUnitSubtree(organizationUuid, organizationUnitUuid)
                .Select(rights => rights.Select(MapOrganizationRightToRolesResponseDTO))
                .Match(Ok, FromOperationError);
        }

        [HttpPost]
        [Route("{organizationUnitUuid}/roles/create")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationUnitRoleAssignmentResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult CreateRoleAssignment(
            [NonEmptyGuid] Guid organizationUnitUuid, [FromBody] CreateOrganizationUnitRoleAssignmentRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _organizationUnitService.CreateRoleAssignment(organizationUnitUuid, request.RoleUuid,
                    request.UserUuid)
                .Select(MapToRoleAssignmentResponse)
                .Match(Ok, FromOperationError);
        }

        [HttpPost]
        [Route("{organizationUnitUuid}/roles/bulk/create")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationUnitResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult CreateBulkRoleAssignment(
            [NonEmptyGuid] Guid organizationUnitUuid, [FromBody] BulkRoleAssignmentRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _organizationUnitService.CreateBulkRoleAssignment(organizationUnitUuid, request.ToUserRolePairs())
                .Select(_responseMapper.ToUnitDto)
                .Match(Ok, FromOperationError);
        }

        [HttpDelete]
        [Route("{organizationUnitUuid}/roles/delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult DeleteRoleAssignment(
            [NonEmptyGuid] Guid organizationUnitUuid, [FromBody] DeleteOrganizationUnitRoleAssignmentRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = _organizationUnitService.DeleteRoleAssignment(organizationUnitUuid, request.RoleUuid,
                    request.UserUuid);

            return result.Ok ? Ok() : FromOperationError(result.Error);
        }

        private CreatedNegotiatedContentResult<OrganizationUnitResponseDTO> MapUnitCreatedResponse(OrganizationUnitResponseDTO dto)
        {
            return Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{dto.Uuid}", dto);
        }

        private static  OrganizationUnitRolesResponseDTO MapOrganizationRightToRolesResponseDTO(OrganizationUnitRight right)
        {
            return new OrganizationUnitRolesResponseDTO
            {
                RoleAssignment = right.MapExtendedRoleAssignmentResponse(),
                OrganizationUnitUuid = right.Object.Uuid,
                OrganizationUnitName = right.Object.Name,
            };
        }

        private static OrganizationUnitRoleAssignmentResponseDTO MapToRoleAssignmentResponse(OrganizationUnitRight right)
        {
            return new OrganizationUnitRoleAssignmentResponseDTO
            {
                RoleUuid = right.Role.Uuid,
                UserUuid = right.User.Uuid,
            };
        }
    }
}