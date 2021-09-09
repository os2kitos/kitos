using Core.DomainModel.ItContract;
using Presentation.Web.Models.API.V2.Response.Contract;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping
{
    public interface IItContractResponseMapper
    {
        ItContractResponseDTO MapContractDTO(ItContract contract);
    }
}
