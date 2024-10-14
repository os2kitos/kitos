

using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.LocalOptions;
using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using Presentation.Web.Models.API.V2.Internal.Response;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.Internal.ItSystems
{
    [RoutePrefix("api/v2/internal/it-systems/{organizationUuid}/local-choice-types")]
    public class ItSystemLocalOptionTypesInternalV2Controller: InternalApiV2Controller
    {

        private readonly IGenericLocalOptionsService<LocalBusinessType, ItSystem, BusinessType> _businessTypeService;
        private readonly ILocalOptionTypeResponseMapper _responseMapper;
        private readonly ILocalOptionTypeWriteModelMapper _writeModelMapper;

        public ItSystemLocalOptionTypesInternalV2Controller(IGenericLocalOptionsService<LocalBusinessType, ItSystem, BusinessType> businessTypeService, ILocalOptionTypeResponseMapper responseMapper, ILocalOptionTypeWriteModelMapper writeModelMapper)
        {
            _businessTypeService = businessTypeService;
            _responseMapper = responseMapper;
            _writeModelMapper = writeModelMapper;
        }

        [HttpGet]
        [Route("business-types")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<LocalRegularOptionResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetBusinessTypes([NonEmptyGuid][FromUri] Guid organizationUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _businessTypeService.GetByOrganizationUuid(organizationUuid)
                .Select(_responseMapper.ToLocalRegularOptionDTOs<ItSystem, BusinessType>)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("business-types/{optionId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(LocalRegularOptionResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetBusinessTypeByOptionId([NonEmptyGuid][FromUri] Guid organizationUuid, [FromUri] int optionId)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _businessTypeService.GetByOrganizationUuidAndOptionId(organizationUuid, optionId)
                .Select(_responseMapper.ToLocalRegularOptionDTO<ItSystem, BusinessType>)
                .Match(Ok, FromOperationError);
        }

        [HttpPost]
        [Route("business-types/")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(LocalRegularOptionResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult CreateBusinessType([NonEmptyGuid][FromUri] Guid organizationUuid, LocalRegularOptionCreateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParameters = _writeModelMapper.ToLocalOptionCreateParameters(dto);

            return _businessTypeService.CreateLocalOption(organizationUuid, updateParameters)
                .Select(_responseMapper.ToLocalRegularOptionDTO<ItSystem, BusinessType>)
                .Match(Ok, FromOperationError);
        }

    }
}