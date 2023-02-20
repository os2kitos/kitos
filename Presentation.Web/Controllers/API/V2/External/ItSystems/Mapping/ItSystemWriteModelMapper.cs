using System;
using System.Collections.Generic;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Model.System;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.System.RightsHolder;
using Presentation.Web.Models.API.V2.Request.System.Shared;

namespace Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping
{
    public class ItSystemWriteModelMapper : WriteModelMapperBase, IItSystemWriteModelMapper
    {
        public ItSystemWriteModelMapper(ICurrentHttpRequest currentHttpRequest) : base(currentHttpRequest)
        {
        }

        public SystemCreationParameters FromRightsHolderPOST(RightsHolderFullItSystemRequestDTO request)
        {
            var creationParameters = new SystemCreationParameters { RightsHolderProvidedUuid = request.Uuid };
            MapChanges(request, creationParameters, true);
            return creationParameters;
        }

        public SystemUpdateParameters FromRightsHolderPUT(RightsHolderFullItSystemRequestDTO request)
        {
            var parameters = new SystemUpdateParameters();
            MapChanges(request, parameters, true);
            return parameters;
        }

        public SystemUpdateParameters FromRightsHolderPATCH(RightsHolderUpdateSystemPropertiesRequestDTO request)
        {
            var parameters = new SystemUpdateParameters();
            MapChanges(request, parameters, false);
            return parameters;
        }

        private void MapChanges(IItSystemWriteRequestCommonPropertiesDTO source, SystemUpdateParameters destination, bool enforceResetOnMissingProperty)
        {
            var rule = CreateChangeRule<IRightsHolderWritableSystemPropertiesRequestDTO>(enforceResetOnMissingProperty);

            destination.Name = rule.MustUpdate(x => x.Name) ? source.Name.AsChangedValue() : OptionalValueChange<string>.None;
            destination.ParentSystemUuid = rule.MustUpdate(x => x.ParentUuid) ? source.ParentUuid.AsChangedValue() : OptionalValueChange<Guid?>.None;
            destination.FormerName = rule.MustUpdate(x => x.FormerName) ? source.FormerName.AsChangedValue() : OptionalValueChange<string>.None;
            destination.Description = rule.MustUpdate(x => x.Description) ? source.Description.AsChangedValue() : OptionalValueChange<string>.None;
            destination.BusinessTypeUuid = rule.MustUpdate(x => x.BusinessTypeUuid) ? source.BusinessTypeUuid.AsChangedValue() : OptionalValueChange<Guid?>.None;
            destination.TaskRefUuids = rule.MustUpdate(x => x.KLEUuids) ? (source.KLEUuids ?? new List<Guid>()).AsChangedValue() : OptionalValueChange<IEnumerable<Guid>>.None;

            switch (source)
            {
                case IHasExternalReferencesCreation createReferences:
                    destination.ExternalReferences = (createReferences.ExternalReferences ?? Array.Empty<ExternalReferenceDataWriteRequestDTO>()).Transform(MapReferences).FromNullable();
                    break;
                case IHasExternalReferencesUpdate updateReferences:
                    var externalReferenceDataDtos = WithResetDataIfPropertyIsDefined(updateReferences.ExternalReferences, nameof(IHasExternalReferencesUpdate.ExternalReferences), () => new List<UpdateExternalReferenceDataWriteRequestDTO>(),enforceResetOnMissingProperty);
                    destination.ExternalReferences = externalReferenceDataDtos.FromNullable().Select(MapUpdateReferences);
                    break;
                default:
                    destination.ExternalReferences = destination.ExternalReferences;
                    break;
            }
        }
        private IEnumerable<UpdatedExternalReferenceProperties> MapReferences(IEnumerable<ExternalReferenceDataWriteRequestDTO> references)
        {
            return BaseMapCreateReferences(references);
        }

        private IEnumerable<UpdatedExternalReferenceProperties> MapUpdateReferences(IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> references)
        {
            return BaseMapUpdateReferences(references);
        }
    }
}