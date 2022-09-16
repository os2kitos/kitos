using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystemUsage;

namespace Presentation.Web.Models.API.V1.ItSystemUsage
{
    public class ItSystemUsageValidationDetailsResponseDTO
    {
        public bool Valid { get; }
        public IEnumerable<ItSystemUsageValidationError> Errors { get; }

        public ItSystemUsageValidationDetailsResponseDTO(bool valid, IEnumerable<ItSystemUsageValidationError> errors)
        {
            Valid = valid;
            Errors = errors.ToList();
        }
    }
}