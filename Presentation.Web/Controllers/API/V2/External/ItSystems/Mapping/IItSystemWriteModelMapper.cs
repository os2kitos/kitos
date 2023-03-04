using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Model.System;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using Presentation.Web.Models.API.V2.Request.System.RightsHolder;

namespace Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping
{
    public interface IItSystemWriteModelMapper
    {
        RightsHolderSystemCreationParameters FromRightsHolderPOST(RightsHolderFullItSystemRequestDTO dto);
        RightsHolderSystemUpdateParameters FromRightsHolderPUT(RightsHolderFullItSystemRequestDTO dto);
        RightsHolderSystemUpdateParameters FromRightsHolderPATCH(RightsHolderUpdateSystemPropertiesRequestDTO dto);
        SystemUpdateParameters FromPOST(CreateItSystemRequestDTO dto);
        SystemUpdateParameters FromPATCH(UpdateItSystemRequestDTO dto);
        ExternalReferenceProperties MapExternalReference(ExternalReferenceDataWriteRequestDTO externalReferenceData);
    }

}
