using Core.Abstractions.Types;
using Core.DomainModel.ItContract;
using Presentation.Web.Models.API.V2.Types.Contract;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping
{
    public static class ItContractValidationErrorChoiceExtension
    {
        private static readonly EnumMap<ItContractValidationErrorChoice, ItContractValidationError> Mapping;
        static ItContractValidationErrorChoiceExtension()
        {
            Mapping = new EnumMap<ItContractValidationErrorChoice, ItContractValidationError>
            (
                (ItContractValidationErrorChoice.EndDatePassed, ItContractValidationError.EndDatePassed),
                (ItContractValidationErrorChoice.StartDateNotPassed, ItContractValidationError.StartDateNotPassed),
                (ItContractValidationErrorChoice.TerminationPeriodExceeded, ItContractValidationError.TerminationPeriodExceeded),
                (ItContractValidationErrorChoice.InvalidParentContract, ItContractValidationError.InvalidParentContract)
            );
        }

        public static ItContractValidationError ToItContractValidationError(this ItContractValidationErrorChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static ItContractValidationErrorChoice ToItContractValidationErrorChoice(this ItContractValidationError value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}