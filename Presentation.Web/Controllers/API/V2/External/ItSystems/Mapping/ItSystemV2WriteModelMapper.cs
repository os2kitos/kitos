using System;
using Core.ApplicationServices.Model.System;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.System;

namespace Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping
{
    public class ItSystemV2WriteModelMapper : WriteModelMapperBase,IItSystemV2WriteModelMapper
    {
        public ItSystemV2WriteModelMapper(ICurrentHttpRequest currentHttpRequest) : base(currentHttpRequest)
        {
        }

        public RightsHolderSystemCreationParameters FromRightsHolderPOST(RightsHolderCreateItSystemRequestDTO request)
        {
            return new RightsHolderSystemCreationParameters(
                request.Name,
                request.Uuid,
                request.ParentUuid,
                request.FormerName, request.Description, request.UrlReference, request.BusinessTypeUuid,
                request.KLENumbers ?? Array.Empty<string>(), request.KLEUuids ?? Array.Empty<Guid>());
        }

        public RightsHolderSystemUpdateParameters FromRightsHolderPUT(RightsHolderWritableITSystemPropertiesDTO request)
        {
            return new RightsHolderSystemUpdateParameters(request.Name, request.ParentUuid, request.FormerName,
                request.Description, request.UrlReference, request.BusinessTypeUuid,
                request.KLENumbers ?? Array.Empty<string>(), request.KLEUuids ?? Array.Empty<Guid>());
        }

        public RightsHolderSystemUpdateParameters FromRightsHolderPATCH(RightsHolderPartialUpdateSystemPropertiesRequestDTO request)
        {
            return new RightsHolderSystemUpdateParameters(request.Name, request.ParentUuid, request.FormerName,
                request.Description, request.UrlReference, request.BusinessTypeUuid,
                request.KLENumbers ?? Array.Empty<string>(), request.KLEUuids ?? Array.Empty<Guid>());
        }

        /*
         * TODO:
         *  - Omskriv modellen så den minder om de andre moduler
         *  - omskriv servicen
         *  - Valider at eksisterende test stadig virker
         *  - implementér patch
         *  - unit test til mapping og service
         *  - nye integrationstest til patch af hver property
         */
    }
}