using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public class ArchivingUpdateRequestDTO : BaseArchivingWriteRequestDTO, IHasJournalPeriods<JournalPeriodUpdateRequestDTO>
    {
        /// <summary>
        /// Updated journal periods
        /// Constraints:
        ///     - If the period has a uuid it will update an existing period (with the same uuid), uuid must exist
        ///     - If the period has no uuid, a new Period will be created
        ///     - Existing periods will be replaced by the input data, so unless identified using uuid in the updates, the existing references will be removed,
        ///       so if you want to retain "identity" of periods between updates make sure to specify the uuid.
        ///       Otherwise the data will be as the input dictates but the uuids will have changed.
        /// </summary>
        public IEnumerable<JournalPeriodUpdateRequestDTO> JournalPeriods { get; set; }
    }
}