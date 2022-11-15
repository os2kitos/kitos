using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Organizations;
using Core.DomainModel.Organization;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1.Organizations;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
{
    [PublicApi]
    [RoutePrefix("api/v1/organizations/{organizationUuid}/organization-units")]

    public class OrganizationUnitLifeCycleController : BaseApiController
    {
        private readonly IOrganizationUnitService _organizationUnitService;

        public OrganizationUnitLifeCycleController(IOrganizationUnitService organizationUnitService)
        {
            _organizationUnitService = organizationUnitService;
        }

        [HttpDelete]
        [Route("{unitUuid}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage Delete(Guid organizationUuid, Guid unitUuid)
        {
            return _organizationUnitService
                .Delete(organizationUuid, unitUuid)
                .Match(FromOperationError, Ok);
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
        [Route("access-rights")]
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