using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.GDPR;
using Core.DomainModel.GDPR;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1.GDPR;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    [RoutePrefix("api/v1/data-processing-registration")]
    public class DataProcessingRegistrationValidationController : BaseApiController
    {
        private readonly IDataProcessingRegistrationApplicationService _dprService;

        public DataProcessingRegistrationValidationController(IDataProcessingRegistrationApplicationService dprService)
        {
            _dprService = dprService;
        }

        [HttpGet]
        [Route("{dprId}/validation-details")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetValidationStatus(int dprId)
        {
            return _dprService
                .Get(dprId)
                .Select(dpr => dpr.CheckDprValidity())
                .Select(MapToDTO)
                .Match(Ok, FromOperationError);
        }

        private static DataProcessingRegistrationValidationDetailsResponseDTO MapToDTO(DataProcessingRegistrationValidationResult validation) => 
            new(validation.Result, validation.ValidationErrors);
    }
}