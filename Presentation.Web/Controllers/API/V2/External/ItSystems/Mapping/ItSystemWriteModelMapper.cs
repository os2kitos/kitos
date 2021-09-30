using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.System;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.System;

namespace Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping
{
    public class ItSystemWriteModelMapper : WriteModelMapperBase, IItSystemWriteModelMapper
    {
        public ItSystemWriteModelMapper(ICurrentHttpRequest currentHttpRequest) : base(currentHttpRequest)
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
            bool ShouldChange<TProperty>(Expression<Func<IRightsHolderWritableSystemPropertiesRequestDTO, TProperty>> pickProperty) => ClientRequestsChangeTo(pickProperty) || enforceResetOnMissingProperty;

            destination.Name = ShouldChange(x => x.Name) ? source.Name.AsChangedValue() : OptionalValueChange<string>.None;
            destination.ParentSystemUuid = ShouldChange(x => x.ParentUuid) ? source.ParentUuid.AsChangedValue() : OptionalValueChange<Guid?>.None;
            destination.FormerName = ShouldChange(x => x.FormerName) ? source.FormerName.AsChangedValue() : OptionalValueChange<string>.None;
            destination.Description = ShouldChange(x => x.Description) ? source.Description.AsChangedValue() : OptionalValueChange<string>.None;
            destination.UrlReference = ShouldChange(x => x.UrlReference) ? source.UrlReference.AsChangedValue() : OptionalValueChange<string>.None;
            destination.BusinessTypeUuid = ShouldChange(x => x.BusinessTypeUuid) ? source.BusinessTypeUuid.AsChangedValue() : OptionalValueChange<Guid?>.None;
            destination.TaskRefKeys = ShouldChange(x => x.KLENumbers) ? (source.KLENumbers ?? new List<string>()).AsChangedValue() : OptionalValueChange<IEnumerable<string>>.None;
            destination.TaskRefUuids = ShouldChange(x => x.KLEUuids) ? (source.KLEUuids ?? new List<Guid>()).AsChangedValue() : OptionalValueChange<IEnumerable<Guid>>.None;
        }
    }
}