using System;
using System.Collections.Generic;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.System;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.System;

namespace Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping
{
    public class ItSystemV2WriteModelMapper : WriteModelMapperBase, IItSystemV2WriteModelMapper
    {
        public ItSystemV2WriteModelMapper(ICurrentHttpRequest currentHttpRequest) : base(currentHttpRequest)
        {
        }

        public RightsHolderSystemCreationParameters FromRightsHolderPOST(RightsHolderCreateItSystemRequestDTO request)
        {
            var creationParameters = new RightsHolderSystemCreationParameters { RightsHolderProvidedUuid = request.Uuid };
            MapChanges(request, creationParameters, true);
            return creationParameters;
        }

        public RightsHolderSystemUpdateParameters FromRightsHolderPUT(RightsHolderWritableITSystemPropertiesDTO request)
        {
            var parameters = new RightsHolderSystemUpdateParameters();
            MapChanges(request, parameters, true);
            return parameters;
        }

        public RightsHolderSystemUpdateParameters FromRightsHolderPATCH(RightsHolderPartialUpdateSystemPropertiesRequestDTO request)
        {
            var parameters = new RightsHolderSystemUpdateParameters();
            MapChanges(request, parameters, false);
            return parameters;
        }

        private void MapChanges(IRightsHolderWritableSystemPropertiesRequestDTO source, RightsHolderSystemUpdateParameters destination, bool enforceResetOnMissingProperty)
        {
            bool ShouldChange(string propertyName) => ClientRequestsChangeTo(propertyName) || enforceResetOnMissingProperty;

            destination.Name = ShouldChange(nameof(RightsHolderWritableITSystemPropertiesDTO.Name)) ? source.Name.AsChangedValue() : OptionalValueChange<string>.None;
            destination.ParentSystemUuid = ShouldChange(nameof(RightsHolderWritableITSystemPropertiesDTO.ParentUuid)) ? source.ParentUuid.AsChangedValue() : OptionalValueChange<Guid?>.None;
            destination.FormerName = ShouldChange(nameof(RightsHolderWritableITSystemPropertiesDTO.FormerName)) ? source.FormerName.AsChangedValue() : OptionalValueChange<string>.None;
            destination.Description = ShouldChange(nameof(RightsHolderWritableITSystemPropertiesDTO.Description)) ? source.Description.AsChangedValue() : OptionalValueChange<string>.None;
            destination.UrlReference = ShouldChange(nameof(RightsHolderWritableITSystemPropertiesDTO.UrlReference)) ? source.UrlReference.AsChangedValue() : OptionalValueChange<string>.None;
            destination.BusinessTypeUuid = ShouldChange(nameof(RightsHolderWritableITSystemPropertiesDTO.BusinessTypeUuid)) ? source.BusinessTypeUuid.AsChangedValue() : OptionalValueChange<Guid?>.None;
            destination.TaskRefKeys = ShouldChange(nameof(RightsHolderWritableITSystemPropertiesDTO.KLENumbers)) ? (source.KLENumbers ?? new List<string>()).AsChangedValue() : OptionalValueChange<IEnumerable<string>>.None;
            destination.TaskRefUuids = ShouldChange(nameof(RightsHolderWritableITSystemPropertiesDTO.KLEUuids)) ? (source.KLEUuids ?? new List<Guid>()).AsChangedValue() : OptionalValueChange<IEnumerable<Guid>>.None;
        }
    }
}