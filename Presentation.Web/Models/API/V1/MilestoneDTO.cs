using System;

namespace Presentation.Web.Models.API.V1
{
    public class MilestoneDTO : ItProjectStatusDTO
    {
        /// <summary>
        /// Which date, the state should be reached
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Trafic light status for the state
        /// </summary>
        public int Status { get; set; }
    }
}
