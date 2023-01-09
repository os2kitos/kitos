using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.DomainModel.GDPR;
using Presentation.Web.Models.API.V2.Request.Shared;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Models.API.V2.Request.DataProcessing
{
    public class UpdateDataProcessingRegistrationRequestDTO : DataProcessingRegistrationWriteRequestDTO, IHasNameExternal, IHasExternalReference<UpdateExternalReferenceDataWriteRequestDTO>
    {
        /// <summary>
        /// Name of the registration
        /// Constraints:
        ///     - Max length: 200
        ///     - Name must be unique within the organization
        /// </summary>
        [MaxLength(DataProcessingRegistrationConstraints.MaxNameLength)]
        public string Name { get; set; }
        /// <summary>
        /// External reference definitions
        /// </summary>
        public IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> ExternalReferences { get; set; }
    }
}