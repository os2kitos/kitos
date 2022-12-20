using System.Collections.Generic;
using System.Linq;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingRegistrationValidationResult
    {
        public DataProcessingRegistrationValidationResult(IEnumerable<DataProcessingRegistrationValidationError> validationErrors)
        {
            ValidationErrors = validationErrors.ToList();
            Result = ValidationErrors.Any() == false;
        }

        public IEnumerable<DataProcessingRegistrationValidationError> ValidationErrors { get; }
        public bool Result { get; }
    }
}
