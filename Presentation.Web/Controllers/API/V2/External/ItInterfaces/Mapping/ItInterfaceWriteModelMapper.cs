using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Interface;
using Core.ApplicationServices.Model.Shared;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Interface;
using System;

namespace Presentation.Web.Controllers.API.V2.External.ItInterfaces.Mapping
{
    public class ItInterfaceWriteModelMapper : WriteModelMapperBase, IItInterfaceWriteModelMapper
    {
        public ItInterfaceWriteModelMapper(ICurrentHttpRequest currentHttpRequest)
            : base(currentHttpRequest)
        {
        }

        public RightsHolderItInterfaceUpdateParameters FromPATCH(RightsHolderPartialUpdateItInterfaceRequestDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return Map(dto, false);
        }

        public RightsHolderItInterfaceCreationParameters FromPOST(RightsHolderCreateItInterfaceRequestDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return new RightsHolderItInterfaceCreationParameters(dto.Uuid, Map(dto, true));
        }

        public RightsHolderItInterfaceUpdateParameters FromPUT(RightsHolderWritableItInterfacePropertiesDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return Map(dto, true);
        }

        private RightsHolderItInterfaceUpdateParameters Map<T>(T dto, bool enforceFallbackIfNotProvided) where T : IRightsHolderWritableItInterfacePropertiesDTO
        {
            return new()
            {
                Name = ClientRequestsChangeTo(nameof(IRightsHolderWritableItInterfacePropertiesDTO.Name)) || enforceFallbackIfNotProvided ? dto.Name.AsChangedValue() : OptionalValueChange<string>.None,
                ExposingSystemUuid = ClientRequestsChangeTo(nameof(IRightsHolderWritableItInterfacePropertiesDTO.ExposedBySystemUuid)) || enforceFallbackIfNotProvided ? dto.ExposedBySystemUuid.AsChangedValue() : OptionalValueChange<Guid>.None,
                Description = ClientRequestsChangeTo(nameof(IRightsHolderWritableItInterfacePropertiesDTO.Description)) || enforceFallbackIfNotProvided ? (dto.Description ?? string.Empty).AsChangedValue() : OptionalValueChange<string>.None,
                InterfaceId = ClientRequestsChangeTo(nameof(IRightsHolderWritableItInterfacePropertiesDTO.InterfaceId)) || enforceFallbackIfNotProvided ? (dto.InterfaceId ?? string.Empty).AsChangedValue() : OptionalValueChange<string>.None,
                Version = ClientRequestsChangeTo(nameof(IRightsHolderWritableItInterfacePropertiesDTO.Version)) || enforceFallbackIfNotProvided ? (dto.Version ?? string.Empty).AsChangedValue() : OptionalValueChange<string>.None,
                UrlReference = ClientRequestsChangeTo(nameof(IRightsHolderWritableItInterfacePropertiesDTO.UrlReference)) || enforceFallbackIfNotProvided ? (dto.UrlReference ?? string.Empty).AsChangedValue() : OptionalValueChange<string>.None
            };
        }
    }
}