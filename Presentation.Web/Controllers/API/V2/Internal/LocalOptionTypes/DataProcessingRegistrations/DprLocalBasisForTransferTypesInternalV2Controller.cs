using System;
using System.Collections.Generic;
using System.Net;
using Core.ApplicationServices.LocalOptions;
using Core.DomainModel.GDPR;
using Core.DomainModel.LocalOptions;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using Presentation.Web.Models.API.V2.Internal.Response;
using Microsoft.AspNetCore.Mvc;
using DataProcessingRegistration = Core.DomainModel.GDPR.DataProcessingRegistration;

namespace Presentation.Web.Controllers.API.V2.Internal.LocalOptionTypes.DataProcessingRegistrations
{
    [Route("api/v2/internal/data-processing/{organizationUuid}/local-option-types/basis-for-transfer-types")]
    public class DprLocalBasisForTransferTypesInternalV2Controller(
        IGenericLocalOptionsService<LocalDataProcessingBasisForTransferOption, DataProcessingRegistration,
            DataProcessingBasisForTransferOption> localOptionTypeService,
        ILocalOptionTypeResponseMapper responseMapper,
        ILocalOptionTypeWriteModelMapper writeModelMapper)
        : BaseLocalRegularOptionTypesInternalV2Controller<LocalDataProcessingBasisForTransferOption,
            DataProcessingRegistration, DataProcessingBasisForTransferOption>(localOptionTypeService, responseMapper,
            writeModelMapper)
    {
        [HttpGet]
        [Route("")]
        [ApiResponse(typeof(IEnumerable<LocalRegularOptionResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetLocalBasisForTransferTypes([NonEmptyGuid][FromRoute] Guid organizationUuid)
        {
            return GetAll(organizationUuid);
        }

        [HttpGet]
        [Route("{optionUuid}")]
        [ApiResponse(typeof(LocalRegularOptionResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetLocalBasisForTransferTypeByOptionId([NonEmptyGuid][FromRoute] Guid organizationUuid, [FromRoute] Guid optionUuid)
        {
            return GetSingle(organizationUuid, optionUuid);
        }

        [HttpPost]
        [Route("")]
        [ApiResponse(typeof(LocalRegularOptionResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult CreateLocalBasisForTransferType([NonEmptyGuid][FromRoute] Guid organizationUuid, [FromBody] LocalOptionCreateRequestDTO dto)
        {
            return Create(organizationUuid, dto);
        }

        [HttpPatch]
        [Route("{optionUuid}")]
        [ApiResponse(typeof(LocalRegularOptionResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult PatchLocalBasisForTransferType([NonEmptyGuid][FromRoute] Guid organizationUuid,
            [FromRoute] Guid optionUuid,
            [FromBody] LocalRegularOptionUpdateRequestDTO dto)
        {
            return Patch(organizationUuid, optionUuid, dto);
        }

        [HttpDelete]
        [Route("{optionUuid}")]
        [ApiResponse(typeof(LocalRegularOptionResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult DeleteLocalBasisForTransferType([NonEmptyGuid][FromRoute] Guid organizationUuid,
            [FromRoute] Guid optionUuid)
        {
            return Delete(organizationUuid, optionUuid);
        }
    }
}


