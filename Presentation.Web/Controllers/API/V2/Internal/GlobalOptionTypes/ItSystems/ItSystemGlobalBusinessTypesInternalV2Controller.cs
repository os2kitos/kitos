
using Core.ApplicationServices.GlobalOptions;
using Core.DomainModel.ItSystem;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System;
using Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions;

namespace Presentation.Web.Controllers.API.V2.Internal.GlobalOptionTypes.ItSystems
{
    [RoutePrefix("api/v2/internal/it-systems/global-option-types/business-types")]

    public class ItSystemGlobalBusinessTypesInternalV2Controller: BaseGlobalRegularOptionTypesInternalV2Controller<ItSystem, BusinessType>
    {
        public ItSystemGlobalBusinessTypesInternalV2Controller(IGlobalRegularOptionsService<BusinessType, ItSystem> globalRegularOptionsService, IGlobalOptionTypeResponseMapper responseMapper, IGlobalOptionTypeWriteModelMapper writeModelMapper) : base(globalRegularOptionsService, responseMapper, writeModelMapper)
        {
        }

        [HttpGet]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<GlobalRegularOptionResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetBusinessTypes()
        {
            return GetAll();
        }

        [HttpPost]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(GlobalRegularOptionResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult CreateBusinessType(GlobalRegularOptionCreateRequestDTO dto)
        {
            return Create(dto);
        }

        [HttpPatch]
        [Route("{optionUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(GlobalRegularOptionResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PatchGlobalBusinessType([NonEmptyGuid][FromUri] Guid optionUuid,
            GlobalRegularOptionUpdateRequestDTO dto)
        {
            return Patch(optionUuid, dto);
        }
    }
}