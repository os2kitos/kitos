using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.HelpTexts;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Models.API.V2.Internal.Request;
using Presentation.Web.Models.API.V2.Internal.Response;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.Internal
{
    [RoutePrefix("api/v2/internal/help-texts")]
    public class HelpTextsInternalV2Controller: InternalApiV2Controller
    {
        private readonly IHelpTextApplicationService _helpTextApplicationService;
        private readonly IHelpTextResponseMapper _responseMapper;
        private readonly IHelpTextWriteModelMapper _writeModelMapper;

        public HelpTextsInternalV2Controller(IHelpTextApplicationService helpTextApplicationService, IHelpTextResponseMapper responseMapper, IHelpTextWriteModelMapper writeModelMapper)
        {
            _helpTextApplicationService = helpTextApplicationService;
            _responseMapper = responseMapper;
            _writeModelMapper = writeModelMapper;
        }

        [HttpGet]
        [Route("{key}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(HelpTextResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetSingle([FromUri] string key)
        {
            return _helpTextApplicationService.GetHelpText(key)
                .Select(_responseMapper.ToResponseDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<HelpTextResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetAll()
        {
            var helpTexts = _helpTextApplicationService.GetHelpTexts();
            return Ok(_responseMapper.ToResponseDTOs(helpTexts));
        }

        [HttpPost]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(HelpTextResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Post(HelpTextCreateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var parameters = _writeModelMapper.ToCreateParameters(dto);
            return _helpTextApplicationService.CreateHelpText(parameters)
                .Select(_responseMapper.ToResponseDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpDelete]
        [Route("{key}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(HelpTextResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Delete([FromUri] string key)
        {
            return _helpTextApplicationService.DeleteHelpText(key)
                .Match(FromOperationError, Ok);
        }

        [HttpPatch]
        [Route("{key}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(HelpTextResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Patch([FromUri] string key, HelpTextUpdateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var parameters = _writeModelMapper.ToUpdateParameters(dto);
            return _helpTextApplicationService.PatchHelpText(key, parameters)
                .Select(_responseMapper.ToResponseDTO)
                .Match(Ok, FromOperationError);
        }
    }

}