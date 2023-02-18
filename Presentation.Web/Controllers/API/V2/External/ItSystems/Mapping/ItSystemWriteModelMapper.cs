using System;
using System.Collections.Generic;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.System;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.System;
using Presentation.Web.Models.API.V2.Request.System.RightsHolder;

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

        public RightsHolderSystemUpdateParameters FromRightsHolderPUT(RightsHolderCreateItSystemRequestDTO request)
        {
            var parameters = new RightsHolderSystemUpdateParameters();
            MapChanges(request, parameters, true);
            return parameters;
        }

        public RightsHolderSystemUpdateParameters FromRightsHolderPATCH(RightsHolderUpdateSystemPropertiesRequestDTO request)
        {
            var parameters = new RightsHolderSystemUpdateParameters();
            MapChanges(request, parameters, false);
            return parameters;
        }

        private void MapChanges(IRightsHolderWritableSystemPropertiesRequestDTO source, RightsHolderSystemUpdateParameters destination, bool enforceResetOnMissingProperty)
        {
            var rule = CreateChangeRule<IRightsHolderWritableSystemPropertiesRequestDTO>(enforceResetOnMissingProperty);

            destination.Name = rule.MustUpdate(x => x.Name) ? source.Name.AsChangedValue() : OptionalValueChange<string>.None;
            destination.ParentSystemUuid = rule.MustUpdate(x => x.ParentUuid) ? source.ParentUuid.AsChangedValue() : OptionalValueChange<Guid?>.None;
            destination.FormerName = rule.MustUpdate(x => x.FormerName) ? source.FormerName.AsChangedValue() : OptionalValueChange<string>.None;
            destination.Description = rule.MustUpdate(x => x.Description) ? source.Description.AsChangedValue() : OptionalValueChange<string>.None;
            destination.UrlReference = rule.MustUpdate(x => x.UrlReference) ? source.UrlReference.AsChangedValue() : OptionalValueChange<string>.None;
            destination.BusinessTypeUuid = rule.MustUpdate(x => x.BusinessTypeUuid) ? source.BusinessTypeUuid.AsChangedValue() : OptionalValueChange<Guid?>.None;
            destination.TaskRefKeys = rule.MustUpdate(x => x.KLENumbers) ? (source.KLENumbers ?? new List<string>()).AsChangedValue() : OptionalValueChange<IEnumerable<string>>.None;
            destination.TaskRefUuids = rule.MustUpdate(x => x.KLEUuids) ? (source.KLEUuids ?? new List<Guid>()).AsChangedValue() : OptionalValueChange<IEnumerable<Guid>>.None;
        }
    }
}