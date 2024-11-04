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
        private readonly IGlobalRoleOptionsService<TRoleOptionType, TReferenceType> _globalRoleOptionsService;
        private readonly IGlobalOptionTypeResponseMapper _responseMapper;
        private readonly IGlobalOptionTypeWriteModelMapper _writeModelMapper;

        public BaseGlobalRoleOptionTypesInternalV2Controller(IGlobalRoleOptionsService<TRoleOptionType, TReferenceType> globalRoleOptionsService, IGlobalOptionTypeResponseMapper responseMapper, IGlobalOptionTypeWriteModelMapper writeModelMapper)
        {
            _globalRoleOptionsService = globalRoleOptionsService;
            _responseMapper = responseMapper;
            _writeModelMapper = writeModelMapper;
        }

        protected IHttpActionResult GetAll()
        {
            return _globalRoleOptionsService.GetGlobalOptions()
                .Select(_responseMapper.ToGlobalRoleOptionDTOs<TRoleOptionType, TReferenceType>)
                .Match(Ok, FromOperationError);
        }

        protected IHttpActionResult Create(GlobalRoleOptionCreateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var createParameters = _writeModelMapper.ToGlobalRoleOptionCreateParameters(dto);
            return _globalRoleOptionsService.CreateGlobalOption(createParameters)
                .Select(_responseMapper.ToGlobalRoleOptionDTO<TRoleOptionType, TReferenceType>)
                .Match(Ok, FromOperationError);
        }

        protected IHttpActionResult Patch([NonEmptyGuid][FromUri] Guid optionUuid, GlobalRoleOptionUpdateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParameters = _writeModelMapper.ToGlobalRoleOptionUpdateParameters(dto);
            return _globalRoleOptionsService.PatchGlobalOption(optionUuid, updateParameters)
                .Select(_responseMapper.ToGlobalRoleOptionDTO<TRoleOptionType, TReferenceType>)
                .Match(Ok, FromOperationError);
        }
    }
}