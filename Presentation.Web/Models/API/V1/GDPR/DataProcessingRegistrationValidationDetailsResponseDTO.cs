using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.GDPR;

namespace Presentation.Web.Models.API.V1.GDPR
{
    public class DataProcessingRegistrationValidationDetailsResponseDTO
    {
        public bool Valid { get; }
        public IEnumerable<DataProcessingRegistrationValidationError> Errors { get; }

        public DataProcessingRegistrationValidationDetailsResponseDTO(bool valid, IEnumerable<DataProcessingRegistrationValidationError> errors)
        {
            Valid = valid;
            Errors = errors.ToList();
        }
    }
}