using Core.ApplicationServices.Organizations;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System;
using System.Linq;
using System.Web.Http;
using Core.ApplicationServices.Organizations.Write;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Response.OrganizationUnit;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.OrganizationUnits.Mapping;
using Presentation.Web.Models.API.V2.Request.OrganizationUnit;

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

        public OrganizationUnitsInternalV2Controller(IOrganizationUnitWriteService organizationUnitWriteService,
            IOrganizationUnitWriteModelMapper organizationUnitWriteModelMapper,
            IOrganizationUnitService organizationUnitService)
        {
            _organizationUnitWriteService = organizationUnitWriteService;
            _organizationUnitWriteModelMapper = organizationUnitWriteModelMapper;
            _organizationUnitService = organizationUnitService;
        }

        [Route("{unitUuid}/permissions")]
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(UnitAccessRightsResponseDTO))]
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
                        ToUnitDto(accessRightWithUnit.OrganizationUnit)
                    )
                ))
                .Match(Ok, FromOperationError);
        }

        [Route("create")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationUnitResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult CreateUnit([NonEmptyGuid] Guid organizationUuid, [FromBody] CreateOrganizationUnitRequestDTO parameters)
        {
            return _organizationUnitWriteService.Create(organizationUuid, _organizationUnitWriteModelMapper.FromPOST(parameters))
                .Select(ToUnitDto)
                .Match(Ok, FromOperationError);
        }

        private static OrganizationUnitResponseDTO ToUnitDto(OrganizationUnit unit)
        {
            return new OrganizationUnitResponseDTO
            {
                Uuid = unit.Uuid,
                Name = unit.Name,
                Ean = unit.Ean,
                ParentOrganizationUnit = unit.Parent?.MapIdentityNamePairDTO()
            };
        }
    }
}