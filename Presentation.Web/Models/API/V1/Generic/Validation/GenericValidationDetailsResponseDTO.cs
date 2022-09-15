using Core.DomainModel.ItContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.API.V1.Generic.Validation
{
    public abstract class GenericValidationDetailsResponseDTO<T> where T : struct, IComparable
    {
        public bool Valid { get; }
        public bool EnforcedValid { get; }
        public IEnumerable<T> Errors { get; }

        protected GenericValidationDetailsResponseDTO(bool valid, bool enforcedValid, IEnumerable<T> errors)
        {
            Valid = valid;
            EnforcedValid = enforcedValid;
            Errors = errors.ToList();
        }
    }
}