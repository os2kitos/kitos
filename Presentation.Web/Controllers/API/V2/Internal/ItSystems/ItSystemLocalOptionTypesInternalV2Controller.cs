﻿using System;
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
    [RoutePrefix("api/v2/internal/it-systems/{organizationUuid}/local-option-types")]
    public class ItSystemLocalOptionTypesInternalV2Controller: InternalApiV2Controller
    {
        private readonly IGenericLocalOptionsService<LocalBusinessType, ItSystem, BusinessType> _localBusinessTypeService;
        private readonly ILocalOptionTypeResponseMapper _responseMapper;
        private readonly ILocalOptionTypeWriteModelMapper _writeModelMapper;

        public ItSystemLocalOptionTypesInternalV2Controller(IGenericLocalOptionsService<LocalBusinessType, ItSystem, BusinessType> localBusinessTypeService, ILocalOptionTypeResponseMapper responseMapper, ILocalOptionTypeWriteModelMapper writeModelMapper)
        {
            _localBusinessTypeService = localBusinessTypeService;
            _responseMapper = responseMapper;
            _writeModelMapper = writeModelMapper;
        }
        
        #region BusinessTypes
        [HttpGet]
        [Route("business-types")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<LocalRegularOptionResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetLocalBusinessTypes([NonEmptyGuid][FromUri] Guid organizationUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            var businessTypes = _localBusinessTypeService.GetLocalOptions(organizationUuid);
            return Ok(_responseMapper.ToLocalRegularOptionDTOs<ItSystem, BusinessType>(businessTypes));
        }

        [HttpGet]
        [Route("business-types/{optionUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(LocalRegularOptionResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetLocalBusinessTypeByOptionId([NonEmptyGuid][FromUri] Guid organizationUuid, [FromUri] Guid optionUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _localBusinessTypeService.GetLocalOption(organizationUuid, optionUuid)
                .Select(_responseMapper.ToLocalRegularOptionDTO<ItSystem, BusinessType>)
                .Match(Ok, FromOperationError);
        }

        [HttpPost]
        [Route("business-types")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(LocalRegularOptionResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult CreateLocalBusinessType([NonEmptyGuid][FromUri] Guid organizationUuid, LocalOptionCreateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var createParameters = _writeModelMapper.ToLocalOptionCreateParameters(dto);

            return _localBusinessTypeService.CreateLocalOption(organizationUuid, createParameters)
                .Select(_responseMapper.ToLocalRegularOptionDTO<ItSystem, BusinessType>)
                .Match(Ok, FromOperationError);
        }

        [HttpPatch]
        [Route("business-types/{optionUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(LocalRegularOptionResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PatchLocalBusinessType([NonEmptyGuid][FromUri] Guid organizationUuid,
            [FromUri] Guid optionUuid,
            LocalRegularOptionUpdateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParameters = _writeModelMapper.ToLocalOptionUpdateParameters(dto);

            return _localBusinessTypeService.PatchLocalOption(organizationUuid, optionUuid, updateParameters)
                .Select(_responseMapper.ToLocalRegularOptionDTO<ItSystem, BusinessType>)
                .Match(Ok, FromOperationError);
        }

        [HttpDelete]
        [Route("business-types/{optionUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(LocalRegularOptionResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult DeleteLocalBusinessType([NonEmptyGuid][FromUri] Guid organizationUuid,
            [FromUri] Guid optionUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _localBusinessTypeService.DeleteLocalOption(organizationUuid, optionUuid)
                .Select(_responseMapper.ToLocalRegularOptionDTO<ItSystem, BusinessType>)
                .Match(Ok, FromOperationError);
        }
        #endregion BusinessTypes
    }
}