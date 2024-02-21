using System;
using System.Collections.Generic;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Model.System;
using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using Presentation.Web.Models.API.V2.Request.System.RightsHolder;
using Presentation.Web.Models.API.V2.Request.System.Shared;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping
{
    public class ItSystemWriteModelMapper : WriteModelMapperBase, IItSystemWriteModelMapper
    {
        public ItSystemWriteModelMapper(ICurrentHttpRequest currentHttpRequest) : base(currentHttpRequest)
        {
        }

        public RightsHolderSystemCreationParameters FromRightsHolderPOST(RightsHolderFullItSystemRequestDTO request)
        {
            var creationParameters = new RightsHolderSystemCreationParameters();
            MapRightsHolderCommonChanges(request, creationParameters, true);
            return creationParameters;
        }

        public RightsHolderSystemUpdateParameters FromRightsHolderPUT(RightsHolderFullItSystemRequestDTO request)
        {
            var parameters = new RightsHolderSystemUpdateParameters();
            MapRightsHolderCommonChanges(request, parameters, true);
            return parameters;
        }

        public RightsHolderSystemUpdateParameters FromRightsHolderPATCH(RightsHolderUpdateSystemPropertiesRequestDTO request)
        {
            var parameters = new RightsHolderSystemUpdateParameters();
            MapRightsHolderCommonChanges(request, parameters, false);
            return parameters;
        }

        public SystemUpdateParameters FromPOST(CreateItSystemRequestDTO dto)
        {
            var parameters = new SystemUpdateParameters();
            MapFullChanges(dto, parameters, true);
            return parameters;
        }

        public SystemUpdateParameters FromPATCH(UpdateItSystemRequestDTO dto)
        {
            var parameters = new SystemUpdateParameters();
            MapFullChanges(dto, parameters, false);
            return parameters;
        }

        public ExternalReferenceProperties MapExternalReference(ExternalReferenceDataWriteRequestDTO externalReferenceData)
        {
            return MapCommonReference(externalReferenceData);
        }

        private void MapFullChanges<TDto>(TDto source, SystemUpdateParameters destination, bool enforceResetOnMissingProperty) where TDto :
            IItSystemWriteRequestCommonPropertiesDTO,
            IItSystemWriteRequestPropertiesDTO
        {

            MapCommonChanges(source, destination, enforceResetOnMissingProperty);

            var rule = CreateChangeRule<TDto>(enforceResetOnMissingProperty);
            destination.RightsHolderUuid = rule.MustUpdate(x => x.RightsHolderUuid)
                ? source.RightsHolderUuid.AsChangedValue()
                : OptionalValueChange<Guid?>.None;

            destination.Scope = rule.MustUpdate(x => x.Scope)
                ? (source.Scope ?? RegistrationScopeChoice.Global).FromChoice().AsChangedValue()
                : OptionalValueChange<AccessModifier>.None;

            destination.ArchivingRecommendation = rule.MustUpdate(x => x.RecommendedArchiveDuty)
                ? MapArchivingRecommendation(source, enforceResetOnMissingProperty).AsChangedValue()
                : OptionalValueChange<(OptionalValueChange<ArchiveDutyRecommendationTypes?> recommendation, OptionalValueChange<string> comment)>.None;

            destination.Deactivated = rule.MustUpdate(x => x.Deactivated)
                ? source.Deactivated.AsChangedValue()
                : OptionalValueChange<bool>.None;
        }

        private (OptionalValueChange<ArchiveDutyRecommendationTypes?> recommendation, OptionalValueChange<string> comment) MapArchivingRecommendation<TDto>(TDto source, bool enforceResetOnMissingProperty) where TDto : IItSystemWriteRequestCommonPropertiesDTO, IItSystemWriteRequestPropertiesDTO
        {
            var rule = CreateChangeRule<TDto>(enforceResetOnMissingProperty);
            var recommendedArchiveDutyChoice = rule.MustUpdate(x => x.RecommendedArchiveDuty.Id)
                ? (source.RecommendedArchiveDuty?.Id?.FromChoice()).AsChangedValue()
                : OptionalValueChange<ArchiveDutyRecommendationTypes?>.None;

            var comment = rule.MustUpdate(x => x.RecommendedArchiveDuty.Comment)
                ? (source.RecommendedArchiveDuty?.Comment).AsChangedValue()
                : OptionalValueChange<string>.None;

            return (recommendedArchiveDutyChoice, comment);
        }

        private void MapRightsHolderCommonChanges(IRightsHolderWritableSystemPropertiesRequestDTO source,
            SharedSystemUpdateParameters destination, bool enforceResetOnMissingProperty)
        {
            var rule = CreateChangeRule<IRightsHolderWritableSystemPropertiesRequestDTO>(enforceResetOnMissingProperty);
 
            destination.ExternalUuid = rule.MustUpdate(x => x.ExternalUuid) ? source.ExternalUuid.AsChangedValue() : OptionalValueChange<Guid?>.None;
            MapCommonChanges(source, destination, enforceResetOnMissingProperty);
        }

        private void MapCommonChanges(IItSystemWriteRequestCommonPropertiesDTO source, SharedSystemUpdateParameters destination, bool enforceResetOnMissingProperty)
        {
            var rule = CreateChangeRule<IItSystemWriteRequestCommonPropertiesDTO>(enforceResetOnMissingProperty);

            destination.Name = rule.MustUpdate(x => x.Name) ? source.Name.AsChangedValue() : OptionalValueChange<string>.None;
            destination.ParentSystemUuid = rule.MustUpdate(x => x.ParentUuid) ? source.ParentUuid.AsChangedValue() : OptionalValueChange<Guid?>.None;
            destination.FormerName = rule.MustUpdate(x => x.PreviousName) ? source.PreviousName.AsChangedValue() : OptionalValueChange<string>.None;
            destination.Description = rule.MustUpdate(x => x.Description) ? source.Description.AsChangedValue() : OptionalValueChange<string>.None;
            destination.BusinessTypeUuid = rule.MustUpdate(x => x.BusinessTypeUuid) ? source.BusinessTypeUuid.AsChangedValue() : OptionalValueChange<Guid?>.None;
            destination.TaskRefUuids = rule.MustUpdate(x => x.KLEUuids) ? (source.KLEUuids ?? new List<Guid>()).AsChangedValue() : OptionalValueChange<IEnumerable<Guid>>.None;

            MapExternalReferences(source, destination, enforceResetOnMissingProperty);
        }

        private void MapExternalReferences(IItSystemWriteRequestCommonPropertiesDTO source, SharedSystemUpdateParameters destination, bool enforceResetOnMissingProperty)
        {
            switch (source)
            {
                case IHasExternalReferencesCreation createReferences:
                    destination.ExternalReferences =
                        (createReferences.ExternalReferences ?? Array.Empty<ExternalReferenceDataWriteRequestDTO>())
                        .Transform(MapReferences).FromNullable();
                    break;
                case IHasExternalReferencesUpdate updateReferences:
                    var externalReferenceDataDtos = WithResetDataIfPropertyIsDefined(
                        updateReferences.ExternalReferences,
                        nameof(IHasExternalReferencesUpdate.ExternalReferences),
                        () => new List<UpdateExternalReferenceDataWriteRequestDTO>(),
                        enforceResetOnMissingProperty);
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