using System;
using System.Web.Http;
using Core.ApplicationServices.LocalOptions;
using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Request.Options;

namespace Presentation.Web.Controllers.API.V2.Internal.LocalOptionTypes
{
    public class BaseLocalRoleOptionTypesInternalV2Controller<TLocalOptionType, TReferenceType, TOptionType> : InternalApiV2Controller
        where TLocalOptionType : LocalOptionEntity<TOptionType>, new()
        where TOptionType : OptionEntity<TReferenceType>, IRoleEntity
    {
        private readonly IGenericLocalOptionsService<TLocalOptionType, TReferenceType, TOptionType> _localRoleOptionTypeService;
        private readonly ILocalOptionTypeResponseMapper _responseMapper;
        private readonly ILocalOptionTypeWriteModelMapper _writeModelMapper;

        public BaseLocalRoleOptionTypesInternalV2Controller(IGenericLocalOptionsService<TLocalOptionType, TReferenceType, TOptionType> localRoleOptionTypeService, ILocalOptionTypeResponseMapper responseMapper, ILocalOptionTypeWriteModelMapper writeModelMapper)
        {
            _localRoleOptionTypeService = localRoleOptionTypeService;
            _responseMapper = responseMapper;
            _writeModelMapper = writeModelMapper;
        }

        public IHttpActionResult GetAll([NonEmptyGuid][FromUri] Guid organizationUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            var roleOptions = _localRoleOptionTypeService.GetLocalOptions(organizationUuid);
            return Ok(_responseMapper.ToLocalRoleOptionDTOs<TReferenceType, TOptionType>(roleOptions));
        }

        public IHttpActionResult GetSingle([NonEmptyGuid][FromUri] Guid organizationUuid, [FromUri] Guid optionUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _localRoleOptionTypeService.GetLocalOption(organizationUuid, optionUuid)
                .Select(_responseMapper.ToLocalRoleOptionDTO<TReferenceType, TOptionType>)
                .Match(Ok, FromOperationError);
        }

        public IHttpActionResult Create([NonEmptyGuid][FromUri] Guid organizationUuid, LocalOptionCreateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var createParameters = _writeModelMapper.ToLocalOptionCreateParameters(dto);

            return _localRoleOptionTypeService.CreateLocalOption(organizationUuid, createParameters)
                .Select(_responseMapper.ToLocalRoleOptionDTO<TReferenceType, TOptionType>)
                .Match(Ok, FromOperationError);
        }

        public IHttpActionResult Patch([NonEmptyGuid][FromUri] Guid organizationUuid,
            [FromUri] Guid optionUuid,
            LocalRegularOptionUpdateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParameters = _writeModelMapper.ToLocalOptionUpdateParameters(dto);

            return _localRoleOptionTypeService.PatchLocalOption(organizationUuid, optionUuid, updateParameters)
                .Select(_responseMapper.ToLocalRoleOptionDTO<TReferenceType, TOptionType>)
                .Match(Ok, FromOperationError);
        }

        public IHttpActionResult Delete([NonEmptyGuid][FromUri] Guid organizationUuid,
            [FromUri] Guid optionUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _localRoleOptionTypeService.DeleteLocalOption(organizationUuid, optionUuid)
                .Select(_responseMapper.ToLocalRoleOptionDTO<TReferenceType, TOptionType>)
                .Match(Ok, FromOperationError);
        }
    }
}