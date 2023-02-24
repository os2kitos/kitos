using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.DomainModel.GDPR;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
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
        /// User defined external references.
        /// The external reference marked as "master reference" will be shown in overviews
        /// Constraints:
        ///     - If the list is not empty one (and only one) must be marked as the master reference.
        ///     - If the reference has a uuid it will update an existing reference (with the same uuid), uuid must exist
        ///     - If the reference has no uuid, a new External Reference will be created
        ///     - Existing references will be replaced by the input data, so unless identified using uuid in the updates, the existing references will be removed.
        /// </summary>
        public IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> ExternalReferences { get; set; }
    }
}