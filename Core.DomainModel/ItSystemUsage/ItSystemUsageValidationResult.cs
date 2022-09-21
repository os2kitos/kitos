using System.Collections.Generic;
using System.Linq;

namespace Core.DomainModel.ItSystemUsage
{
    public class ItSystemUsageValidationResult
    {
        public IEnumerable<ItSystemUsageValidationError> ValidationErrors { get; }
        public bool Result { get; }

        public ItSystemUsageValidationResult(IEnumerable<ItSystemUsageValidationError> validationErrors)
        {
            ValidationErrors = validationErrors.ToList();
            Result = ValidationErrors.Any() == false;
        }
    }
}
