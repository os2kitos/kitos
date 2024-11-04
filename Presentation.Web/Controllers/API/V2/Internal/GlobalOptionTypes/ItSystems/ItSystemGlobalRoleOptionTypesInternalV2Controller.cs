using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.GlobalOptions;
using Core.DomainModel.ItSystem;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Request;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.Internal.GlobalOptionTypes.ItSystems
{
    public class ItSystemGlobalRoleOptionTypesInternalV2Controller: BaseGlobalRoleOptionTypesInternalV2Controller<ItSystemRole, ItSystemRight>
    {
        public ItSystemGlobalRoleOptionTypesInternalV2Controller(IGenericGlobalOptionsService<ItSystemRole, ItSystemRight> globalOptionsService, IGlobalOptionTypeResponseMapper responseMapper, IGlobalOptionTypeWriteModelMapper writeModelMapper) : base(globalOptionsService, responseMapper, writeModelMapper)
        {
        }

        [HttpGet]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<GlobalRoleOptionResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItSystemRoles()
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
        public IHttpActionResult CreateItSystemRole(GlobalRoleOptionCreateRequestDTO dto)
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
        public IHttpActionResult PatchGlobalBItSystemRole([NonEmptyGuid][FromUri] Guid optionUuid,
            GlobalRoleOptionUpdateRequestDTO dto)
        {
            return Patch(optionUuid, dto);
        }
    }
}