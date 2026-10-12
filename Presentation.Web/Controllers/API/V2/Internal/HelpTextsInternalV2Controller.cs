using System.Collections.Generic;
using System.Net;
using Core.ApplicationServices.HelpTexts;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Models.API.V2.Internal.Request;
using Presentation.Web.Models.API.V2.Internal.Response;
using Microsoft.AspNetCore.Mvc;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V2.Internal
{
    [Route("api/v2/internal/help-texts")]
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
        [ApiResponse(typeof(HelpTextResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Conflict)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetSingle([FromRoute] string key)
        {
            return _helpTextApplicationService.GetHelpText(key)
                .Select(_responseMapper.ToResponseDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("")]
        [ApiResponse(typeof(IEnumerable<HelpTextResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Conflict)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetAll()
        {
            var helpTexts = _helpTextApplicationService.GetHelpTexts();
            return Ok(_responseMapper.ToResponseDTOs(helpTexts));
        }

        [HttpPost]
        [Route("")]
        [ApiResponse(typeof(HelpTextResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Conflict)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Post([FromBody] HelpTextCreateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var parameters = _writeModelMapper.ToCreateParameters(dto);
            return _helpTextApplicationService.CreateHelpText(parameters)
                .Select(_responseMapper.ToResponseDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpDelete]
        [Route("{key}")]
        [ApiResponse(typeof(HelpTextResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Conflict)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Delete([FromRoute] string key)
        {
            return _helpTextApplicationService.DeleteHelpText(key)
                .Match(FromOperationError, Ok);
        }

        [HttpPatch]
        [Route("{key}")]
        [ApiResponse(typeof(HelpTextResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Conflict)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Patch([FromRoute] string key, [FromBody] HelpTextUpdateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var parameters = _writeModelMapper.ToUpdateParameters(dto);
            return _helpTextApplicationService.PatchHelpText(key, parameters)
                .Select(_responseMapper.ToResponseDTO)
                .Match(Ok, FromOperationError);
        }
    }

}


