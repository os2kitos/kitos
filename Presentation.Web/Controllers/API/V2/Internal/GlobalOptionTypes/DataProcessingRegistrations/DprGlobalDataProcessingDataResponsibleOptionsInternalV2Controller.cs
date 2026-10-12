using Core.ApplicationServices.GlobalOptions;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using System.Collections.Generic;
using System.Net;
using System;
using Core.DomainModel.GDPR;
using Microsoft.AspNetCore.Mvc;
using Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions;

namespace Presentation.Web.Controllers.API.V2.Internal.GlobalOptionTypes.DataProcessingRegistrations
{
    [Route("api/v2/internal/dpr/global-option-types/data-processing-data-responsible-options")]

    public class DprGlobalDataProcessingDataResponsibleOptionsInternalV2Controller: BaseGlobalRegularOptionTypesInternalV2Controller<DataProcessingRegistration, DataProcessingDataResponsibleOption>
    {
        public DprGlobalDataProcessingDataResponsibleOptionsInternalV2Controller(IGlobalRegularOptionsService<DataProcessingDataResponsibleOption, DataProcessingRegistration> globalRegularOptionsService, IGlobalOptionTypeResponseMapper responseMapper, IGlobalOptionTypeWriteModelMapper writeModelMapper) : base(globalRegularOptionsService, responseMapper, writeModelMapper)
        {
        }

        [HttpGet]
        [Route("")]
        [ApiResponse(typeof(IEnumerable<GlobalRegularOptionResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetGlobalDataProcessingDataResponsibleOptions()
        {
            return GetAll();
        }

        [HttpPost]
        [Route("")]
        [ApiResponse(typeof(GlobalRegularOptionResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult CreateGlobalDataProcessingDataResponsibleOption([FromBody] GlobalRegularOptionCreateRequestDTO dto)
        {
            return Create(dto);
        }

        [HttpPatch]
        [Route("{optionUuid}")]
        [ApiResponse(typeof(GlobalRegularOptionResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult PatchGlobalDataProcessingDataResponsibleOption([NonEmptyGuid][FromRoute] Guid optionUuid,
            [FromBody] GlobalRegularOptionUpdateRequestDTO dto)
        {
            return Patch(optionUuid, dto);
        }
    }
}
    


