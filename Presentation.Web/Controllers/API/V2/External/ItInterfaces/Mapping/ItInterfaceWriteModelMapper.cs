using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Interface;
using Core.ApplicationServices.Model.Shared;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Interface;
using System;
using Presentation.Web.Controllers.API.V2.Common.Mapping;

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

            var parameters = new RightsHolderItInterfaceUpdateParameters();
            Map(dto, parameters, false);
            return parameters;
        }

        public RightsHolderItInterfaceCreationParameters FromPOST(RightsHolderCreateItInterfaceRequestDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var parameters = new RightsHolderItInterfaceUpdateParameters();
            Map(dto, parameters, true);
            return new RightsHolderItInterfaceCreationParameters(dto.Uuid, parameters);
        }

        public RightsHolderItInterfaceUpdateParameters FromPUT(RightsHolderWritableItInterfacePropertiesDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var parameters = new RightsHolderItInterfaceUpdateParameters();
            Map(dto, parameters, true);
            return parameters;
        }

        private void Map(IRightsHolderWritableItInterfacePropertiesDTO source, RightsHolderItInterfaceUpdateParameters destination, bool enforceFallbackIfNotProvided)
        {
            var rule = CreateChangeRule<IRightsHolderWritableItInterfacePropertiesDTO>(enforceFallbackIfNotProvided);

            destination.Name = rule.MustUpdate(x=>x.Name) ? source.Name.AsChangedValue() : OptionalValueChange<string>.None;
            destination.InterfaceId = rule.MustUpdate(x => x.InterfaceId) ? (source.InterfaceId ?? string.Empty).AsChangedValue() : OptionalValueChange<string>.None;
            destination.ExposingSystemUuid = rule.MustUpdate(x => x.ExposedBySystemUuid) ? source.ExposedBySystemUuid.AsChangedValue() : OptionalValueChange<Guid>.None;
            destination.Description = rule.MustUpdate(x => x.Description) ? source.Description.AsChangedValue() : OptionalValueChange<string>.None;
            destination.Version = rule.MustUpdate(x => x.Version) ? source.Version.AsChangedValue() : OptionalValueChange<string>.None;
            destination.UrlReference = rule.MustUpdate(x => x.UrlReference) ? source.UrlReference.AsChangedValue() : OptionalValueChange<string>.None;
        }
    }
}