using Presentation.Web.Models.API.V2.Types.Contract;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping
{
    public static class ItContractSupplierTypeMappingExtension
    {
        public static ItContractSupplierTypeChoice ToItContractSupplierTypeChoice(this bool? hasInternalSupplier)
        {
            return hasInternalSupplier switch
            {
                true => ItContractSupplierTypeChoice.Internal,
                false => ItContractSupplierTypeChoice.External,
                null => ItContractSupplierTypeChoice.External
            };
        }
    }
}
