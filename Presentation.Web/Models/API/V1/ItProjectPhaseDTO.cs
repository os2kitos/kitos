using System;

namespace Presentation.Web.Models.API.V1
{
    public class ItProjectPhaseDTO
    {
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
