using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItContract;

namespace Presentation.Web.Models.API.V1.ItContract
{
    public class ContractValidationDetailsResponseDTO
    {
        public bool Valid { get; }
        public bool EnforcedValid { get; }
        public IEnumerable<ItContractValidationError> Errors { get; }

        public ContractValidationDetailsResponseDTO(bool valid, bool enforcedValid, IEnumerable<ItContractValidationError> errors)
        {
            Valid = valid;
            EnforcedValid = enforcedValid;
            Errors = errors.ToList();
        }
    }
}