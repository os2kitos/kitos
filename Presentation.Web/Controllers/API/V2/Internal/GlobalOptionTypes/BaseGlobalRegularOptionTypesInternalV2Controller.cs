

using System;
using System.Web.Http;
using Core.ApplicationServices.GlobalOptions;
using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Request.Options;

namespace Presentation.Web.Controllers.API.V2.Internal.GlobalOptionTypes
{
    public class BaseGlobalRegularOptionTypesInternalV2Controller<TReferenceType, TOptionType> : InternalApiV2Controller
        where TOptionType : OptionEntity<TReferenceType>, new()
    {
        private readonly IGlobalRegularOptionsService<TOptionType, TReferenceType> _globalRegularOptionsService;
        private readonly IGlobalOptionTypeResponseMapper _responseMapper;
        private readonly IGlobalOptionTypeWriteModelMapper _writeModelMapper;

        public BaseGlobalRegularOptionTypesInternalV2Controller(IGlobalRegularOptionsService<TOptionType, TReferenceType> globalRegularOptionsService, IGlobalOptionTypeResponseMapper responseMapper, IGlobalOptionTypeWriteModelMapper writeModelMapper)
        {
            _globalRegularOptionsService = globalRegularOptionsService;
            _responseMapper = responseMapper;
            _writeModelMapper = writeModelMapper;
        }

        protected IHttpActionResult GetAll()
        {
            return _globalRegularOptionsService.GetGlobalOptions()
                .Select(_responseMapper.ToGlobalRegularOptionDTOs<TOptionType, TReferenceType>)
                .Match(Ok, FromOperationError);
        }

        protected IHttpActionResult Create(GlobalRegularOptionCreateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var createParameters = _writeModelMapper.ToGlobalRegularOptionCreateParameters(dto);
            return _globalRegularOptionsService.CreateGlobalOption(createParameters)
                .Select(_responseMapper.ToGlobalRegularOptionDTO<TOptionType, TReferenceType>)
                .Match(Ok, FromOperationError);
        }

        protected IHttpActionResult Patch([NonEmptyGuid][FromUri] Guid optionUuid, GlobalRegularOptionUpdateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParameters = _writeModelMapper.ToGlobalRegularOptionUpdateParameters(dto);
            return _globalRegularOptionsService.PatchGlobalOption(optionUuid, updateParameters)
                .Select(_responseMapper.ToGlobalRegularOptionDTO<TOptionType, TReferenceType>)
                .Match(Ok, FromOperationError);
        }
    }
}