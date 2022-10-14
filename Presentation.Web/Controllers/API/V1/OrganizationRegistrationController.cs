using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.Abstractions.Types;
using Core.ApplicationServices.Organizations;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
{
    [PublicApi]
    [RoutePrefix("api/v1/unit-registrations")]

    public class OrganizationRegistrationController: BaseApiController
    {
        private readonly IOrganizationRegistrationService _organizationRegistrationService;
        
        public OrganizationRegistrationController(IOrganizationRegistrationService organizationRegistrationService)
        {
            _organizationRegistrationService = organizationRegistrationService;
        }

        [HttpGet]
        [Route("{unitId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetUnitRegistrations(int unitId)
        {
            return _organizationRegistrationService.GetOrganizationRegistrations(unitId)
                .Match(Ok,
                    err =>
                    {
                        return err.FailureType switch
                        {
                            OperationFailure.Forbidden => Unauthorized(err.Message.GetValueOrDefault()),
                            OperationFailure.BadInput => BadRequest(err.Message.GetValueOrDefault()),
                            _ => BadRequest()
                        };
                    });
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