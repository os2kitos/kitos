using System.Collections.Generic;
using System.Linq;

namespace Core.DomainModel.ItSystemUsage
{
    public class ItSystemUsageValidationResult
    {
        public IEnumerable<ItSystemUsageValidationError> ValidationErrors { get; }
        public bool Result { get; }

        public ItSystemUsageValidationResult(IEnumerable<ItSystemUsageValidationError> validationErrors, bool valid)
        {
            ValidationErrors = validationErrors.ToList();
            Result = valid;
        }
    }
}
