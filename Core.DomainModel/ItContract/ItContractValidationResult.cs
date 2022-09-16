using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Shared;

namespace Core.DomainModel.ItContract
{
    public class ItContractValidationResult
    {
        public IEnumerable<ItContractValidationError> ValidationErrors { get; }
        public bool Result { get; }
        public bool EnforcedValid { get; }

        public ItContractValidationResult(bool enforcedValid, IEnumerable<ItContractValidationError> validationErrors)
        {
            ValidationErrors = validationErrors.ToList();
            Result = enforcedValid || validationErrors.Any() == false;
            EnforcedValid = enforcedValid;
        }
    }
}
