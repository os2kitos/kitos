using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.Shared;

namespace Core.DomainModel.ItSystemUsage
{
    public class ItSystemUsageValidationResult : GenericValidationResult<ItSystemUsageValidationError>
    {
        public ItSystemUsageValidationResult(bool enforcedValid, IEnumerable<ItSystemUsageValidationError> validationErrors) : base(enforcedValid, validationErrors)
        {
        }
    }
}
