using Core.ApplicationServices.GlobalOptions;
using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using System.Web.Http;
using System;
using Presentation.Web.Models.API.V2.Internal.Request;

namespace Presentation.Web.Controllers.API.V2.Internal.GlobalOptionTypes
{
    public class BaseGlobalRoleOptionTypesInternalV2Controller<TRoleOptionType, TReferenceType> : InternalApiV2Controller
        where TRoleOptionType : OptionEntity<TReferenceType>, IRoleEntity, new()
    {
        private readonly IGlobalRegularOptionsService<TRoleOptionType, TReferenceType> _globalRegularOptionsService;
        private readonly IGlobalOptionTypeResponseMapper _responseMapper;
        private readonly IGlobalOptionTypeWriteModelMapper _writeModelMapper;

        public BaseGlobalRoleOptionTypesInternalV2Controller(IGlobalRegularOptionsService<TRoleOptionType, TReferenceType> globalRegularOptionsService, IGlobalOptionTypeResponseMapper responseMapper, IGlobalOptionTypeWriteModelMapper writeModelMapper)
        {
            _globalRegularOptionsService = globalRegularOptionsService;
            _responseMapper = responseMapper;
            _writeModelMapper = writeModelMapper;
        }

        protected IHttpActionResult GetAll()
        {
            return _globalRegularOptionsService.GetGlobalOptions()
                .Select(_responseMapper.ToGlobalRoleOptionDTOs<TRoleOptionType, TReferenceType>)
                .Match(Ok, FromOperationError);
        }

        protected IHttpActionResult Create(GlobalRoleOptionCreateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var createParameters = _writeModelMapper.ToGlobalRoleOptionCreateParameters(dto);
            return _globalRegularOptionsService.CreateGlobalOption(createParameters)
                .Select(_responseMapper.ToGlobalRoleOptionDTO<TRoleOptionType, TReferenceType>)
                .Match(Ok, FromOperationError);
        }

        protected IHttpActionResult Patch([NonEmptyGuid][FromUri] Guid optionUuid, GlobalRoleOptionUpdateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParameters = _writeModelMapper.ToGlobalRoleOptionUpdateParameters(dto);
            return _globalRegularOptionsService.PatchGlobalOption(optionUuid, updateParameters)
                .Select(_responseMapper.ToGlobalRoleOptionDTO<TRoleOptionType, TReferenceType>)
                .Match(Ok, FromOperationError);
        }
    }
}