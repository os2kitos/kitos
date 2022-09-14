using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Shared;

namespace Core.DomainModel.ItContract
{
    public class ItContractValidationResult : GenericValidationResult<ItContractValidationError>
    {
        public ItContractValidationResult(bool enforcedValid, IEnumerable<ItContractValidationError> validationErrors) : base(enforcedValid, validationErrors)
        {
        }
    }
}
