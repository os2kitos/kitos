using Core.Abstractions.Types;
using Core.DomainModel.GDPR;
using Presentation.Web.Models.API.V2.Types.DataProcessing;

namespace Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping
{
    public static class DataProcessingRegistrationValidationErrorChoiceExtension
    {
        private static readonly EnumMap<DataProcessingRegistrationValidationErrorChoice, DataProcessingRegistrationValidationError> Mapping;

        static DataProcessingRegistrationValidationErrorChoiceExtension()
        {
            Mapping = new EnumMap<DataProcessingRegistrationValidationErrorChoice, DataProcessingRegistrationValidationError>
            (
                (DataProcessingRegistrationValidationErrorChoice.MainContractNotActive, DataProcessingRegistrationValidationError.MainContractNotActive),
                (DataProcessingRegistrationValidationErrorChoice.EnforcedInvalidity, DataProcessingRegistrationValidationError.EnforcedInvalidity)
            );
        }

        public static DataProcessingRegistrationValidationError ToDataProcessingRegistrationValidationError(this DataProcessingRegistrationValidationErrorChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static DataProcessingRegistrationValidationErrorChoice ToDataProcessingRegistrationValidationErrorChoice(this DataProcessingRegistrationValidationError value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}
