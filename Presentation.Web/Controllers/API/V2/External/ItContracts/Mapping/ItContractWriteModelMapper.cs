using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Contracts.Write;
using Presentation.Web.Models.API.V2.Request.Contract;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping
{
    public class ItContractWriteModelMapper : IItContractWriteModelMapper
    {
        public ItContractModificationParameters FromPOST(ContractWriteRequestDTO dto)
        {
            return Map(dto);
        }

        public ItContractModificationParameters FromPUT(ContractWriteRequestDTO dto)
        {
            return Map(dto);
        }

        private static ItContractModificationParameters Map(ContractWriteRequestDTO dto)
        {
            return new ItContractModificationParameters
            {
                Name = dto.Name.AsChangedValue()
            };
        }
    }
}