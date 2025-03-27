using System.Collections.Generic;
using System.Linq;

namespace Core.DomainModel.ItContract
{
    public class ItContractValidationResult
    {
        public IEnumerable<ItContractValidationError> ValidationErrors { get; }
        public bool Result { get; }
        public bool EnforcedValid { get; }
        public bool RequireValidParent { get; set; }

        public ItContractValidationResult(bool enforcedValid, bool requireValidParent, bool parentIsValid, IEnumerable<ItContractValidationError> validationErrors)

        {
            var errors = validationErrors.ToList();
            var validityResult = requireValidParent ? parentIsValid : errors.Any() == false;
            ValidationErrors = errors;
            Result = enforcedValid || validityResult;
            EnforcedValid = enforcedValid;
            RequireValidParent = requireValidParent;
        }

    }
}
