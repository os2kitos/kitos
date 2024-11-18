using Core.ApplicationServices.GlobalOptions;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using Presentation.Web.Models.API.V2.Internal.Request;
using Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Core.DomainModel.GDPR;

namespace Presentation.Web.Controllers.API.V2.Internal.GlobalOptionTypes.DataProcessingRegistrations
{
        [RoutePrefix("api/v2/internal/dpr/global-option-types/dpr-roles")]
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
            [Route]
            [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<GlobalRoleOptionResponseDTO>))]
            [SwaggerResponse(HttpStatusCode.BadRequest)]
            [SwaggerResponse(HttpStatusCode.Unauthorized)]
            [SwaggerResponse(HttpStatusCode.Forbidden)]
            [SwaggerResponse(HttpStatusCode.NotFound)]
            public IHttpActionResult GetDprRoles()
            {
                return GetAll();
            }

            [HttpPost]
            [Route]
            [SwaggerResponse(HttpStatusCode.OK, Type = typeof(GlobalRoleOptionResponseDTO))]
            [SwaggerResponse(HttpStatusCode.BadRequest)]
            [SwaggerResponse(HttpStatusCode.Unauthorized)]
            [SwaggerResponse(HttpStatusCode.Forbidden)]
            [SwaggerResponse(HttpStatusCode.NotFound)]
            public IHttpActionResult CreateDprRole(GlobalRoleOptionCreateRequestDTO dto)
            {
                return Create(dto);
            }

            [HttpPatch]
            [Route("{optionUuid}")]
            [SwaggerResponse(HttpStatusCode.OK, Type = typeof(GlobalRoleOptionResponseDTO))]
            [SwaggerResponse(HttpStatusCode.BadRequest)]
            [SwaggerResponse(HttpStatusCode.Unauthorized)]
            [SwaggerResponse(HttpStatusCode.Forbidden)]
            [SwaggerResponse(HttpStatusCode.NotFound)]
            public IHttpActionResult PatchDprRole([NonEmptyGuid] [FromUri] Guid optionUuid,
                GlobalRoleOptionUpdateRequestDTO dto)
            {
                return Patch(optionUuid, dto);
            }

        }
}