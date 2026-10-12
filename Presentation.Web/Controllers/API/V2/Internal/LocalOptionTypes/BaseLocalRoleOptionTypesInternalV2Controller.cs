using System;
using Core.ApplicationServices.LocalOptions;
using Core.DomainModel;
using Core.DomainModel.LocalOptions;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Microsoft.AspNetCore.Mvc;
using Presentation.Web.Models.API.V2.Internal.Request.Options;

namespace Presentation.Web.Controllers.API.V2.Internal.LocalOptionTypes
{
    public class BaseLocalRoleOptionTypesInternalV2Controller<TLocalOptionType, TReferenceType, TOptionType> : InternalApiV2Controller
        where TLocalOptionType : LocalRoleOptionEntity<TOptionType>, new()
        where TOptionType : OptionEntity<TReferenceType>, IRoleEntity
    {
        private readonly IGenericLocalRoleOptionsService<TLocalOptionType, TReferenceType, TOptionType> _localRoleOptionTypeService;
        private readonly ILocalOptionTypeResponseMapper _responseMapper;
        private readonly ILocalOptionTypeWriteModelMapper _writeModelMapper;

        public BaseLocalRoleOptionTypesInternalV2Controller(IGenericLocalRoleOptionsService<TLocalOptionType, TReferenceType, TOptionType> localRoleOptionTypeService, ILocalOptionTypeResponseMapper responseMapper, ILocalOptionTypeWriteModelMapper writeModelMapper)
        {
            _localRoleOptionTypeService = localRoleOptionTypeService;
            _responseMapper = responseMapper;
            _writeModelMapper = writeModelMapper;
        }

        protected IActionResult GetAll([NonEmptyGuid][FromQuery] Guid organizationUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            var roleOptions = _localRoleOptionTypeService.GetLocalOptions(organizationUuid);
            return Ok(_responseMapper.ToLocalRoleOptionDTOs<TReferenceType, TOptionType>(roleOptions));
        }

        protected IActionResult GetSingle([NonEmptyGuid][FromQuery] Guid organizationUuid, [FromQuery] Guid optionUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _localRoleOptionTypeService.GetLocalOption(organizationUuid, optionUuid)
                .Select(_responseMapper.ToLocalRoleOptionDTO<TReferenceType, TOptionType>)
                .Match(Ok, FromOperationError);
        }

        protected IActionResult Create([NonEmptyGuid][FromQuery] Guid organizationUuid, LocalOptionCreateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var createParameters = _writeModelMapper.ToLocalOptionCreateParameters(dto);

            return _localRoleOptionTypeService.CreateLocalOption(organizationUuid, createParameters)
                .Select(_responseMapper.ToLocalRoleOptionDTO<TReferenceType, TOptionType>)
                .Match(Ok, FromOperationError);
        }

        protected IActionResult Patch([NonEmptyGuid][FromQuery] Guid organizationUuid,
            [FromQuery] Guid optionUuid,
            LocalRoleOptionUpdateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParameters = _writeModelMapper.ToLocalRoleOptionUpdateParameters(dto);

            return _localRoleOptionTypeService.PatchLocalOption(organizationUuid, optionUuid, updateParameters)
                .Select(_responseMapper.ToLocalRoleOptionDTO<TReferenceType, TOptionType>)
                .Match(Ok, FromOperationError);
        }

        protected IActionResult Delete([NonEmptyGuid][FromQuery] Guid organizationUuid,
            [FromQuery] Guid optionUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _localRoleOptionTypeService.DeleteLocalOption(organizationUuid, optionUuid)
                .Select(_responseMapper.ToLocalRoleOptionDTO<TReferenceType, TOptionType>)
                .Match(Ok, FromOperationError);
        }
    }
}

