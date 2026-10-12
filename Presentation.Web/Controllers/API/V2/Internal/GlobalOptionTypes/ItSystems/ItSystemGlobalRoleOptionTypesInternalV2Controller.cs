using System;
using System.Collections.Generic;
using System.Net;
using Core.ApplicationServices.GlobalOptions;
using Core.DomainModel.ItSystem;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Request;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.Internal.GlobalOptionTypes.ItSystems
{
    [Route("api/v2/internal/it-systems/global-option-types/it-systems-roles")]
    public class ItSystemGlobalRoleOptionTypesInternalV2Controller: BaseGlobalRoleOptionTypesInternalV2Controller<ItSystemRole, ItSystemRight>
    {
        public ItSystemGlobalRoleOptionTypesInternalV2Controller(IGlobalRoleOptionsService<ItSystemRole, ItSystemRight> globalRoleOptionsService, IGlobalOptionTypeResponseMapper responseMapper, IGlobalOptionTypeWriteModelMapper writeModelMapper) : base(globalRoleOptionsService, responseMapper, writeModelMapper)
        {
        }

        [HttpGet]
        [Route("")]
        [ApiResponse(typeof(IEnumerable<GlobalRoleOptionResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetItSystemRoles()
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
        public IActionResult CreateItSystemRole([FromBody] GlobalRoleOptionCreateRequestDTO dto)
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
        public IActionResult PatchGlobalBItSystemRole([NonEmptyGuid][FromRoute] Guid optionUuid,
            [FromBody] GlobalRoleOptionUpdateRequestDTO dto)
        {
            return Patch(optionUuid, dto);
        }
    }
}


