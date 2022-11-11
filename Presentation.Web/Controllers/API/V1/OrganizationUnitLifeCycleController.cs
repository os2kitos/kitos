using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Organizations;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
{
    [PublicApi]
    [RoutePrefix("api/v1/organizations/{organizationUuid}/organization-units/{unitUuid}")]

    public class OrganizationUnitLifeCycleController : BaseApiController
    {
        private readonly IOrganizationUnitService _organizationUnitService;

        public OrganizationUnitLifeCycleController(IOrganizationUnitService organizationUnitService)
        {
            _organizationUnitService = organizationUnitService;
        }

        [HttpDelete]
        [Route("")]
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
    }
}