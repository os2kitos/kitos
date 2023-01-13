using Presentation.Web.Models.API.V2.Request.Shared;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public class UpdateItSystemUsageRequestDTO : BaseItSystemUsageWriteRequestDTO
    {
        public GeneralDataUpdateRequestDTO General { get; set; }
        /// <summary>
        /// User defined external references.
        /// The external reference marked as "master reference" will be shown in overviews and on the system front page in KITOS
        /// Constraints:
        ///     - If the list is not empty one (and only one) must be marked as the master reference.
        ///     - If the reference has a uuid it will update an existing reference (with the same uuid), uuid must exist
        ///     - If the reference has no uuid, a new External Reference will be created
        ///     - Existing references will be replaced by the input data in this field unless specifically identified using the uuid on items in the update
        /// </summary>
        public IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> ExternalReferences { get; set; }
    }
}