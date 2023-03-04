using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Interface;
using Core.ApplicationServices.Model.Shared;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Interface;
using System;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.External.Generic;
using System.Collections.Generic;

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

        public ItInterfaceWriteModel FromPOST(CreateItInterfaceRequestDTO request)
        {
            var writeModel = new ItInterfaceWriteModel();
            Map(request, writeModel, true);
            return writeModel;
        }

        public ItInterfaceWriteModel FromPATCH(UpdateItInterfaceRequestDTO request)
        {
            var writeModel = new ItInterfaceWriteModel();
            Map(request, writeModel, false);
            return writeModel;
        }

        private void Map(IItInterfaceWritablePropertiesRequestDTO source, ItInterfaceWriteModel destination, bool enforceFallbackIfNotProvided)
        {
            var rule = CreateChangeRule<IItInterfaceWritablePropertiesRequestDTO>(enforceFallbackIfNotProvided);
            MapCommon(source, destination, enforceFallbackIfNotProvided);
            destination.ExposingSystemUuid = rule.MustUpdate(x => x.ExposedBySystemUuid) ? source.ExposedBySystemUuid.AsChangedValue() : OptionalValueChange<Guid?>.None;
            destination.Note = rule.MustUpdate(x => x.Note) ? source.Note.AsChangedValue() : OptionalValueChange<string>.None;
            destination.Deactivated = rule.MustUpdate(x => x.Deactivated) ? source.Deactivated.AsChangedValue() : OptionalValueChange<bool>.None;
            destination.InterfaceTypeUuid = rule.MustUpdate(x => x.ItInterfaceTypeUuid) ? source.ItInterfaceTypeUuid.AsChangedValue() : OptionalValueChange<Guid?>.None;
            destination.Scope = rule.MustUpdate(x => x.Scope) ? source.Scope.FromChoice().AsChangedValue() : OptionalValueChange<AccessModifier>.None;
            destination.Data = rule.MustUpdate(x => x.Data)
                ? (source.Data ?? Array.Empty<ItInterfaceDataRequestDTO>())
                .Select(x => new ItInterfaceDataWriteModel(x.Description, x.DataTypeUuid))
                .ToList()
                .AsChangedValue<IReadOnlyList<ItInterfaceDataWriteModel>>()
                : OptionalValueChange<IReadOnlyList<ItInterfaceDataWriteModel>>.None;
        }

        private void Map(IRightsHolderWritableItInterfacePropertiesDTO source, ItInterfaceWriteModelParametersBase destination, bool enforceFallbackIfNotProvided)
        {
            var rule = CreateChangeRule<IRightsHolderWritableItInterfacePropertiesDTO>(enforceFallbackIfNotProvided);

            destination.ExposingSystemUuid = rule.MustUpdate(x => x.ExposedBySystemUuid) ? ((Guid?)source.ExposedBySystemUuid).AsChangedValue() : OptionalValueChange<Guid?>.None;

            MapCommon(source, destination, enforceFallbackIfNotProvided);
        }

        private void MapCommon(ICommonItInterfaceRequestPropertiesDTO source, ItInterfaceWriteModelParametersBase destination, bool enforceFallbackIfNotProvided)
        {
            var rule = CreateChangeRule<ICommonItInterfaceRequestPropertiesDTO>(enforceFallbackIfNotProvided);
            destination.Name = rule.MustUpdate(x => x.Name) ? source.Name.AsChangedValue() : OptionalValueChange<string>.None;
            destination.InterfaceId = rule.MustUpdate(x => x.InterfaceId)
                ? (source.InterfaceId ?? string.Empty).AsChangedValue()
                : OptionalValueChange<string>.None;

            destination.Description = rule.MustUpdate(x => x.Description)
                ? source.Description.AsChangedValue()
                : OptionalValueChange<string>.None;

            destination.Version = rule.MustUpdate(x => x.Version)
                ? source.Version.AsChangedValue()
                : OptionalValueChange<string>.None;

            destination.UrlReference = rule.MustUpdate(x => x.UrlReference)
                ? source.UrlReference.AsChangedValue()
                : OptionalValueChange<string>.None;
        }
    }
}