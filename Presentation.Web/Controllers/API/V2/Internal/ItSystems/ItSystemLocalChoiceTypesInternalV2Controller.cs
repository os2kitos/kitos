

using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.LocalOptions;
using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Response.Options;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.Internal.ItSystems
{
    [RoutePrefix("api/v2/internal/it-systems/{organizationUuid}/local-choice-types")]
    public class ItSystemLocalChoiceTypesInternalV2Controller: InternalApiV2Controller
    {

        private readonly IGenericLocalOptionsService<LocalBusinessType, ItSystem, BusinessType> _businessTypeService;
        private readonly IOptionTypeResponseMapper _responseMapper;

        public ItSystemLocalChoiceTypesInternalV2Controller(IGenericLocalOptionsService<LocalBusinessType, ItSystem, BusinessType> businessTypeService, IOptionTypeResponseMapper responseMapper)
        {
            _businessTypeService = businessTypeService;
            _responseMapper = responseMapper;
        }

        [HttpGet]
        [Route("business-types")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<RegularOptionResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetBusinessTypes([NonEmptyGuid][FromUri] Guid organizationUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _businessTypeService.GetByOrganizationUuid(organizationUuid)
                .Select(_responseMapper.ToRegularOptionDTOs<ItSystem, BusinessType>)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("business-types/{optionId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<RegularOptionResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetBusinessTypeByOptionId([NonEmptyGuid][FromUri] Guid organizationUuid, [FromUri] int optionId)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _businessTypeService.GetByOrganizationUuidAndOptionId(organizationUuid, optionId)
                .Select(_responseMapper.ToRegularOptionDTO<ItSystem, BusinessType>)
                .Match(Ok, FromOperationError);
        }
    }
}