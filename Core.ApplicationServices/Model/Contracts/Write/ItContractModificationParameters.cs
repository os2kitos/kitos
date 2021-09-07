using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractModificationParameters
    {
        public OptionalValueChange<string> Name { get; set; } = OptionalValueChange<string>.None;
    }
}
