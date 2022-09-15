using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Models.API.V1.Generic.Validation;

namespace Presentation.Web.Models.API.V1.ItSystemUsage
{
    public class ItSystemUsageValidationDetailsResponseDTO : GenericValidationDetailsResponseDTO<ItSystemUsageValidationError>
    {
        public ItSystemUsageValidationDetailsResponseDTO(bool valid, bool enforcedValid, IEnumerable<ItSystemUsageValidationError> errors) : base(valid, enforcedValid, errors)
        {
        }
    }
}