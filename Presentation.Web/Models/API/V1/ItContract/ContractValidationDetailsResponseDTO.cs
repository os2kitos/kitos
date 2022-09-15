using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItContract;
using Presentation.Web.Models.API.V1.Generic.Validation;

namespace Presentation.Web.Models.API.V1.ItContract
{
    public class ContractValidationDetailsResponseDTO: GenericValidationDetailsResponseDTO<ItContractValidationError>
    {
        public ContractValidationDetailsResponseDTO(bool valid, bool enforcedValid, IEnumerable<ItContractValidationError> errors) : base(valid, enforcedValid, errors)
        {
        }
    }
}