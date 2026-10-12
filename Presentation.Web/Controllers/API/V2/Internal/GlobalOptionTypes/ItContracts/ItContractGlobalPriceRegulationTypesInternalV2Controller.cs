using System;
using System.Collections.Generic;
using System.Net;
using Core.ApplicationServices.GlobalOptions;
using Core.DomainModel.ItContract;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.Internal.GlobalOptionTypes.ItContracts
{
    [Route("api/v2/internal/it-contract/global-option-types/price-regulation-types")]

    public class ItContractGlobalPriceRegulationTypesInternalV2Controller: BaseGlobalRegularOptionTypesInternalV2Controller<ItContract, PriceRegulationType>
    {
        public ItContractGlobalPriceRegulationTypesInternalV2Controller(IGlobalRegularOptionsService<PriceRegulationType, ItContract> globalRegularOptionsService, IGlobalOptionTypeResponseMapper responseMapper, IGlobalOptionTypeWriteModelMapper writeModelMapper) : base(globalRegularOptionsService, responseMapper, writeModelMapper)
        {
        }

        [HttpGet]
        [Route("")]
        [ApiResponse(typeof(IEnumerable<GlobalRegularOptionResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetGlobalPriceRegulationTypes()
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
        public IActionResult CreateGlobalPriceRegulationType([FromBody] GlobalRegularOptionCreateRequestDTO dto)
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
        public IActionResult PatchGlobalPriceRegulationType([NonEmptyGuid][FromRoute] Guid optionUuid,
            [FromBody] GlobalRegularOptionUpdateRequestDTO dto)
        {
            return Patch(optionUuid, dto);
        }
    }
}
    


