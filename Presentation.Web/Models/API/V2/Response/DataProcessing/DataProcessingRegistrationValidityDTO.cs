using Presentation.Web.Models.API.V2.Types.DataProcessing;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Response.DataProcessing
{
    public class DataProcessingRegistrationValidityDTO
    {
        /// <summary>
        /// Determines if the entity is considered valid.
        /// </summary>
        public required bool Valid { get; set; }
        /// <summary>
        /// Determines if this entity has been forced into invalid state even if context properties would dictate otherwise
        /// </summary>
        public required bool EnforceInvalidity { get; set; }
        /// <summary>
        /// Reasons as to why the registration is considered to be invalid
        /// </summary>
        public required IEnumerable<DataProcessingRegistrationValidationErrorChoice> ValidationErrors { get; set; }
    }
}
