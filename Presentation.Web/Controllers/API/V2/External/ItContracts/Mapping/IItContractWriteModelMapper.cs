using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.Model.Shared.Write;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping
{
    public interface IItContractWriteModelMapper
    {
        ItContractModificationParameters FromPOST(CreateNewContractRequestDTO dto);
        ItContractModificationParameters FromPATCH(UpdateContractRequestDTO dto);
        ItContractModificationParameters FromPUT(UpdateContractRequestDTO dto);
        ExternalReferenceProperties MapExternalReference(ExternalReferenceDataWriteRequestDTO externalReferenceData);
    }
}
