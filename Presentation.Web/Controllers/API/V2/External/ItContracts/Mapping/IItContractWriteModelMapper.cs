using Core.ApplicationServices.Model.Contracts.Write;
using Presentation.Web.Models.API.V2.Request.Contract;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping
{
    public interface IItContractWriteModelMapper
    {
        ItContractModificationParameters FromPOST(CreateNewContractRequestDTO dto);
        ItContractModificationParameters FromPUT(UpdateContractRequestDTO dto);
        ItContractGeneralDataModificationParameters MapGeneralData(ContractGeneralDataWriteRequestDTO generalDataDto);
    }
}
