using System.Collections.Generic;
using System.Linq;

namespace Core.DomainModel.ItContract
{
    public class ItContractValidationResult
    {
        public IEnumerable<ItContractValidationError> ValidationErrors { get; }
        public bool Result { get; }
        public bool EnforcedValid { get; }

        public ItContractValidationResult(bool enforcedValid, IEnumerable<ItContractValidationError> validationErrors)

        {
            var errors = validationErrors.ToList();
            ValidationErrors = errors;
            Result = enforcedValid || errors.Any() == false;
            EnforcedValid = enforcedValid;
        }
    }
}
