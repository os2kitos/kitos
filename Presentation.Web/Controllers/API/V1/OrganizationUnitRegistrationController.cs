using System.Net;
using System.Net.Http;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
{
    [PublicApi]
    [RoutePrefix("api/v1/unit-registrations")]

    public class OrganizationUnitRegistrationController: BaseApiController
    {
        [HttpGet]
        [Route("{unitId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetUnitRegistrations(int unitId)
        {
            return Ok();
        }

        [HttpDelete]
        [Route("{unitId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage RemoveSelectedUnitRegistrations(int unitId)
        {
            return Ok();
        }

        [HttpPut]
        [Route("{unitId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage TransferSelectedUnitRegistrations(int unitId)
        {
            return Ok();
        }
    }
}