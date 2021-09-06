using System;
using Presentation.Web.Models.API.V2.Types.Contract;

namespace Presentation.Web.Models.API.V2.Request.Contract
{
    public class ContractTerminationTermsRequestDTO
    {
        /// <summary>
        /// Optionally assigned notice period
        /// Constraints:
        ///     - If changed from current state, the option type must be available in the organization
        /// </summary>
        public Guid? NoticePeriodMonthsUuid { get; set; }
        /// <summary>
        /// Elaborates the selection in NoticePeriod 
        /// </summary>
        public YearSegmentChoice? NoticePeriodExtendsCurrent { get; set; }
        /// <summary>
        /// Defines a fixed termination notice schema
        /// </summary>
        public YearSegmentChoice? NoticeByEndOf { get; set; }
    }
}