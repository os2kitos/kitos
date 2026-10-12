using Core.ApplicationServices.Organizations;
using Presentation.Web.Infrastructure.Attributes;
using System.Net;
using System;
using System.Linq;
using Core.ApplicationServices.Organizations.Write;
using Presentation.Web.Models.API.V2.Internal.Response.OrganizationUnit;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Controllers.API.V2.Internal.OrganizationUnits.Mapping;
using Presentation.Web.Models.API.V2.Request.OrganizationUnit;
using System.Collections.Generic;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Microsoft.AspNetCore.Mvc;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;

namespace Presentation.Web.Controllers.API.V2.Internal.OrganizationUnits
{
    /// <summary>
    /// Internal API for the organization units in KITOS
    /// </summary>
    [Route("api/v2/internal/organizations/{organizationUuid}/organization-units")]
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
        [ApiResponse(typeof(UnitAccessRightsResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult GetPermissions([NonEmptyGuid] Guid organizationUuid, [NonEmptyGuid] Guid unitUuid)
        {
            return _organizationUnitService.GetAccessRights(organizationUuid, unitUuid)
                .Select(accessRights => new UnitAccessRightsResponseDTO(accessRights))
                .Match(Ok, FromOperationError);
        }

        [Route("all/collection-permissions")]
        [HttpGet]
        [ApiResponse(typeof(UnitAccessRightsWithUnitDataResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult GetCollectionPermissions([NonEmptyGuid] Guid organizationUuid)
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
        [ApiResponse(typeof(OrganizationUnitResponseDTO), HttpStatusCode.Created)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult CreateUnit([NonEmptyGuid] Guid organizationUuid, [FromBody] CreateOrganizationUnitRequestDTO parameters)
        {
            return _organizationUnitWriteService.Create(organizationUuid, _organizationUnitWriteModelMapper.FromPOST(parameters))
                .Select(_responseMapper.ToUnitDto)
                .Match(MapUnitCreatedResponse, FromOperationError);
        }

        [Route("{organizationUnitUuid}/patch")]
        [HttpPatch]
        [ApiResponse(typeof(OrganizationUnitResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult PatchUnit([NonEmptyGuid] Guid organizationUuid, [NonEmptyGuid] Guid organizationUnitUuid, [FromBody] UpdateOrganizationUnitRequestDTO parameters)
        {
            return _organizationUnitWriteService.Patch(organizationUuid, organizationUnitUuid,
                    _organizationUnitWriteModelMapper.FromPATCH(parameters))
                .Select(_responseMapper.ToUnitDto)
                .Match(Ok, FromOperationError);
        }

        [Route("{organizationUnitUuid}/delete")]
        [HttpDelete]
        [ApiResponse(HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult DeleteUnit([NonEmptyGuid] Guid organizationUuid, [NonEmptyGuid] Guid organizationUnitUuid)
        {
            var result = _organizationUnitService.Delete(organizationUuid, organizationUnitUuid);
            return result.HasValue ? FromOperationError(result.Value) : Ok();
        }

        [HttpGet]
        [Route("{organizationUnitUuid}/roles")]
        [ApiResponse(typeof(IEnumerable<OrganizationUnitRolesResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult GetRoleAssignments([NonEmptyGuid] Guid organizationUuid, [NonEmptyGuid] Guid organizationUnitUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _organizationUnitService.GetRightsOfUnitSubtree(organizationUuid, organizationUnitUuid)
                .Select(rights => rights.Select(MapOrganizationRightToRolesResponseDTO))
                .Match(Ok, FromOperationError);
        }

        [HttpPost]
        [Route("{organizationUnitUuid}/roles/create")]
        [ApiResponse(typeof(OrganizationUnitRoleAssignmentResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult CreateRoleAssignment(
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
        [ApiResponse(typeof(OrganizationUnitResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult CreateBulkRoleAssignment(
            [NonEmptyGuid] Guid organizationUnitUuid, [FromBody] BulkRoleAssignmentRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _organizationUnitService.CreateBulkRoleAssignment(organizationUnitUuid, request.ToUserRolePairs())
                .Select(_responseMapper.ToUnitDto)
                .Match(Ok, FromOperationError);
        }

        [HttpDelete]
        [Consumes("application/json")]
        [Route("{organizationUnitUuid}/roles/delete")]
        [ApiResponse(HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult DeleteRoleAssignment(
            [NonEmptyGuid] Guid organizationUnitUuid, [FromBody] DeleteOrganizationUnitRoleAssignmentRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = _organizationUnitService.DeleteRoleAssignment(organizationUnitUuid, request.RoleUuid,
                    request.UserUuid);

            return result.Ok ? Ok() : FromOperationError(result.Error);
        }

        private IActionResult MapUnitCreatedResponse(OrganizationUnitResponseDTO dto)
        {
            return Created($"{new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}").AbsoluteUri.TrimEnd('/')}/{dto.Uuid}", dto);
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


