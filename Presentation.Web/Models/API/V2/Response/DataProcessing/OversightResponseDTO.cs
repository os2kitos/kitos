using System;
using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Types.DataProcessing;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Response.DataProcessing
{
    public class OversightResponseDTO
    {
        /// <summary>
        /// Applied oversight options.
        /// </summary>
        public IEnumerable<Guid> OversightOptions { get; set; }
        /// <summary>
        /// Remark related to the selected oversight options
        /// </summary>
        public string OversightOptionsRemark { get; set; }
        /// <summary>
        /// Determines the interval of the oversight activity
        /// </summary>
        public OversightIntervalChoice? OversightInterval { get; set; }
        /// <summary>
        /// Remark regarding the oversight interval
        /// </summary>
        public string OversightIntervalRemark { get; set; }
        /// <summary>
        /// Determines if an oversight activity has been completed
        /// </summary>
        public YesNoUndecidedChoice? IsOversightCompleted { get; set; }
        /// <summary>
        /// Remark related to the oversight completion
        /// </summary>
        public string OversightCompletedRemark { get; set; }
        /// <summary>
        /// Specific dates where the oversight activity took place
        public IEnumerable<OversightDateDTO> OversightDates { get; set; }
    }
}