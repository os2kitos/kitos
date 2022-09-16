using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Models.API.V1.ItSystemUsage;
using Swashbuckle.Swagger.Annotations;
using System.Net.Http;
using System.Net;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1
{
    [PublicApi]
    [RoutePrefix("api/v1/itsystemusage")]
    public class ItSystemUsageValidationController : BaseApiController
    {
        private readonly IItSystemUsageService _itSystemUsageService;
        
        public ItSystemUsageValidationController(IItSystemUsageService itSystemUsageService)
        {
            _itSystemUsageService = itSystemUsageService;
        }

        [HttpGet]
        [Route("{usageId}/validation-details")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetValidationStatus(int usageId)
        {
            var systemUsageValidity = _itSystemUsageService.GetById(usageId).CheckSystemValidity();

            return Ok(MapToValidationResponseDTO(systemUsageValidity));
        }

        private static ItSystemUsageValidationDetailsResponseDTO MapToValidationResponseDTO(ItSystemUsageValidationResult validation) =>
            new(validation.Result, validation.EnforcedValid,
                validation.ValidationErrors);
    }
}