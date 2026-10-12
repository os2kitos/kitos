using Core.ApplicationServices.GlobalOptions;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using Presentation.Web.Models.API.V2.Internal.Request;
using Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions;
using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Core.DomainModel.GDPR;

namespace Presentation.Web.Controllers.API.V2.Internal.GlobalOptionTypes.DataProcessingRegistrations
{
        [Route("api/v2/internal/dpr/global-option-types/dpr-roles")]
        public class DprGlobalRoleOptionTypesInternalV2Controller : BaseGlobalRoleOptionTypesInternalV2Controller<
            DataProcessingRegistrationRole, DataProcessingRegistrationRight>
        {
        public DprGlobalRoleOptionTypesInternalV2Controller
            (
            IGlobalRoleOptionsService<DataProcessingRegistrationRole, DataProcessingRegistrationRight>
                    globalRoleOptionsService, IGlobalOptionTypeResponseMapper responseMapper,
                IGlobalOptionTypeWriteModelMapper writeModelMapper) : base(globalRoleOptionsService, responseMapper,
                writeModelMapper)
            {
            }

            [HttpGet]
            [Route("")]
            [ApiResponse(typeof(IEnumerable<GlobalRoleOptionResponseDTO>), HttpStatusCode.OK)]
            [ApiResponse(HttpStatusCode.BadRequest)]
            [ApiResponse(HttpStatusCode.Unauthorized)]
            [ApiResponse(HttpStatusCode.Forbidden)]
            [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetDprRoles()
            {
                return GetAll();
            }

            [HttpPost]
            [Route("")]
            [ApiResponse(typeof(GlobalRoleOptionResponseDTO), HttpStatusCode.OK)]
            [ApiResponse(HttpStatusCode.BadRequest)]
            [ApiResponse(HttpStatusCode.Unauthorized)]
            [ApiResponse(HttpStatusCode.Forbidden)]
            [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult CreateDprRole([FromBody] GlobalRoleOptionCreateRequestDTO dto)
            {
                return Create(dto);
            }

            [HttpPatch]
            [Route("{optionUuid}")]
            [ApiResponse(typeof(GlobalRoleOptionResponseDTO), HttpStatusCode.OK)]
            [ApiResponse(HttpStatusCode.BadRequest)]
            [ApiResponse(HttpStatusCode.Unauthorized)]
            [ApiResponse(HttpStatusCode.Forbidden)]
            [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult PatchDprRole([NonEmptyGuid] [FromRoute] Guid optionUuid,
                [FromBody] GlobalRoleOptionUpdateRequestDTO dto)
            {
                return Patch(optionUuid, dto);
            }

        }
}


