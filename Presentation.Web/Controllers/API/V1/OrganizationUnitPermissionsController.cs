using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System;
using System.Linq;
using System.Web.Http;
using Core.DomainModel.Organization;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1.Organizations;
using Swashbuckle.Swagger.Annotations;
using Core.ApplicationServices.Organizations;

namespace Presentation.Web.Controllers.API.V1
{
    [PublicApi]
    [RoutePrefix("api/v1/organizations/{organizationUuid}/organization-units")]
    public class OrganizationUnitPermissionsController : BaseApiController
    {
        private readonly IOrganizationUnitService _organizationUnitService;

        public OrganizationUnitPermissionsController(IOrganizationUnitService organizationUnitService)
        {
            _organizationUnitService = organizationUnitService;
        }

        [HttpGet]
        [Route("{unitUuid}/access-rights")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetUnitAccessRights(Guid organizationUuid, Guid unitUuid)
        {
            return _organizationUnitService.GetAccessRights(organizationUuid, unitUuid)
                .Select(ToAccessRightsDto)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("all/access-rights")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetUnitAccessRightsForOrganization(Guid organizationUuid)
        {
            return _organizationUnitService.GetAccessRightsByOrganization(organizationUuid)
                .Select(ToUnitWithAccessRightsDtoAccessRightsDto)
                .Match(Ok, FromOperationError);
        }

        private static UnitAccessRightsDTO ToAccessRightsDto(
            UnitAccessRights accessRights)
        {
            return new UnitAccessRightsDTO(
                accessRights.CanBeRead,
                accessRights.CanBeModified,
                accessRights.CanBeRenamed,
                accessRights.CanEanBeModified,
                accessRights.CanDeviceIdBeModified,
                accessRights.CanBeRearranged,
                accessRights.CanBeDeleted);
        }

        private static IEnumerable<UnitWithAccessRightsDTO> ToUnitWithAccessRightsDtoAccessRightsDto(
            IEnumerable<UnitWithAccessRights> accessRights)
        {
            return accessRights.Select(x =>
                new UnitWithAccessRightsDTO(
                    x.OrganizationUnit.Id,
                    x.UnitAccessRights.CanBeRead,
                    x.UnitAccessRights.CanBeModified,
                    x.UnitAccessRights.CanBeRenamed,
                    x.UnitAccessRights.CanEanBeModified,
                    x.UnitAccessRights.CanDeviceIdBeModified,
                    x.UnitAccessRights.CanBeRearranged,
                    x.UnitAccessRights.CanBeDeleted)
            );
        }
    }
}