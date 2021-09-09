using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractModificationParameters
    {
        public OptionalValueChange<string> Name { get; set; } = OptionalValueChange<string>.None;
        public Maybe<ItContractGeneralDataModificationParameters> General { get; set; } = Maybe<ItContractGeneralDataModificationParameters>.None;
    }
}
