using Core.ApplicationServices.LocalOptions;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using System.Web.Http;
using System;
using Core.DomainModel;

namespace Presentation.Web.Controllers.API.V2.Internal
{
    public class LocalRegularOptionTypesInternalV2Controller<TLocalOptionType, TReferenceType, TOptionType> : InternalApiV2Controller 
        where TLocalOptionType : LocalOptionEntity<TOptionType>, new()
        where TOptionType : OptionEntity<TReferenceType>
    {
        private readonly IGenericLocalOptionsService<TLocalOptionType, TReferenceType, TOptionType> _localOptionTypeService;
        private readonly ILocalOptionTypeResponseMapper _responseMapper;
        private readonly ILocalOptionTypeWriteModelMapper _writeModelMapper;

        public LocalRegularOptionTypesInternalV2Controller(
            IGenericLocalOptionsService<TLocalOptionType, TReferenceType, TOptionType> localOptionTypeService, 
            ILocalOptionTypeResponseMapper responseMapper,
            ILocalOptionTypeWriteModelMapper writeModelMapper)
        {
            _localOptionTypeService = localOptionTypeService;
            _responseMapper = responseMapper;
            _writeModelMapper = writeModelMapper;
        }

        
        protected IHttpActionResult GetAll([NonEmptyGuid][FromUri] Guid organizationUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            var localOptions = _localOptionTypeService.GetLocalOptions(organizationUuid);
            return Ok(_responseMapper.ToLocalRegularOptionDTOs<TReferenceType, TOptionType>(localOptions));
        }

        protected IHttpActionResult GetSingle([NonEmptyGuid][FromUri] Guid organizationUuid, [FromUri] Guid optionUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _localOptionTypeService.GetLocalOption(organizationUuid, optionUuid)
                .Select(_responseMapper.ToLocalRegularOptionDTO<TReferenceType, TOptionType>)
                .Match(Ok, FromOperationError);
        }

        protected IHttpActionResult Create([NonEmptyGuid][FromUri] Guid organizationUuid, LocalOptionCreateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var createParameters = _writeModelMapper.ToLocalOptionCreateParameters(dto);

            return _localOptionTypeService.CreateLocalOption(organizationUuid, createParameters)
                .Select(_responseMapper.ToLocalRegularOptionDTO<TReferenceType, TOptionType>)
                .Match(Ok, FromOperationError);
        }

        protected IHttpActionResult Patch([NonEmptyGuid][FromUri] Guid organizationUuid,
            [FromUri] Guid optionUuid,
            LocalRegularOptionUpdateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParameters = _writeModelMapper.ToLocalOptionUpdateParameters(dto);

            return _localOptionTypeService.PatchLocalOption(organizationUuid, optionUuid, updateParameters)
                .Select(_responseMapper.ToLocalRegularOptionDTO<TReferenceType, TOptionType>)
                .Match(Ok, FromOperationError);
        }

        protected IHttpActionResult Delete([NonEmptyGuid][FromUri] Guid organizationUuid,
            [FromUri] Guid optionUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _localOptionTypeService.DeleteLocalOption(organizationUuid, optionUuid)
                .Select(_responseMapper.ToLocalRegularOptionDTO<TReferenceType, TOptionType>)
                .Match(Ok, FromOperationError);
        }
    }
}