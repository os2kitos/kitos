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
        /// Constraint:
        ///     - If the list is not empty one (and only one) must be marked as the master reference.
        /// </summary>
        public IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> ExternalReferences { get; set; }
    }
}