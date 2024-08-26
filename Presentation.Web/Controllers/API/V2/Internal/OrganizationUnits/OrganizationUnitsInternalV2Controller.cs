using Core.ApplicationServices.Organizations;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System;
using System.Linq;
using System.Web.Http;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Response.OrganizationUnit;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Controllers.API.V2.Common.Mapping;

namespace Presentation.Web.Controllers.API.V2.Internal.OrganizationUnits
{
    /// <summary>
    /// Internal API for the local registrations related to it-systems in KITOS
    /// </summary>
    [RoutePrefix("api/v2/internal/organizations/{organizationUuid}/organization-units")]
    public class OrganizationUnitsInternalV2Controller : InternalApiV2Controller
    {
        private readonly IOrganizationUnitService _organizationUnitService;

        public OrganizationUnitsInternalV2Controller(IOrganizationUnitService organizationUnitService)
        {
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
                        ToUnitDTO(accessRightWithUnit.OrganizationUnit)
                    )
                ))
                .Match(Ok, FromOperationError);
        }

        private OrganizationUnitResponseDTO ToUnitDTO(OrganizationUnit unit)
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